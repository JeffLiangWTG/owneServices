using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CopyOGDToPGADataForProductsProcessor
	{
		public CopyOGDToPGADataForProductsProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}

		readonly ILogger logger;
		const int BatchSize = 50;
		const string GenAddOnColumnCachedName = "CopyOGDToPGAForProduct";

		#region Processor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Process(CancellationToken token)
		{
			logger.Log(LogType.Information, "Start Copy OGD data to PGA on products");
			var sql = "SELECT TOP {0} XA_ParentID, XA_PK from dbo.GenAddOnColumn WHERE XA_Name = '{1}' AND XA_ParentTableCode = 'OP'";
			var hasProductsToTransform = true;
			var transformedCount = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				var factory = new BusinessObjectFactory();
				var orgPartPKs = new DynamicBusinessObjectCollection(factory);
				orgPartPKs.Load(string.Format(CultureInfo.InvariantCulture, sql, BatchSize, GenAddOnColumnCachedName));

				if (orgPartPKs.Count > 0)
				{
					var deleteCommand = new StringBuilder();
					deleteCommand.Append("DELETE dbo.GenAddOnColumn WHERE XA_PK IN (");
					foreach (DynamicBusinessObject decPK in orgPartPKs)
					{
						var orgSupplierPart = factory.Load<OrgSupplierPart>(new ZGuid(decPK[GenAddOnColumnSchema.XA_ParentID]));
						if (orgSupplierPart != null)
						{
							try
							{
								CopyOGDDataToPGA(orgSupplierPart);
							}
							catch (Exception ex)
							{
								logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Tranformation failed for product: {0}.\r\nException: {1}", orgSupplierPart.OP_PartNum, ex));
							}
							deleteCommand.AppendFormat(CultureInfo.InvariantCulture, "'{0}',", decPK[GenAddOnColumnSchema.PK]);
						}
					}

					factory.Save();
					deleteCommand.Remove(deleteCommand.Length - 1, 1);
					deleteCommand.Append(")");
					Db.Connection.ExecuteNonQuery(deleteCommand.ToString());

					transformedCount += orgPartPKs.Count;
				}
				else
				{
					SetMoveOGDToPGADataDateTimeToEmpty();
					hasProductsToTransform = false;
				}
			} while (hasProductsToTransform);

			if (transformedCount > 0)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Successfully copied OGD data to PGA for {0} products.", transformedCount));
			}
			else
			{
				logger.Log(LogType.Information, "There is no product to be transformed.");
			}
		}

		void CopyOGDDataToPGA(OrgSupplierPart product)
		{
			var pivots = product.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Canada);
			var pivotsNeedTransform = pivots.Where(p => ShouldTransformForCFIA(p) || ShouldTransformForTires(p));

			if (pivotsNeedTransform.Any())
			{
				foreach (var pivot in pivots)
				{
					CopyCFIAOGDToPGA(pivot);
					CopyTiresOGDToPGA(pivot);
				}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				product.Logs.AddNew(ZArchitecture.Business.AutoEvents.EditedARecord, "Copy OGD data to PGA");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		bool ShouldTransformForCFIA(CusClassPartPivot pivot)
		{
			return pivot.CCA_CFIAIndicator.IsEmpty
				&& (!pivot.CCA_AirsCode.IsEmpty
					|| !pivot.CCA_EndUse.IsEmpty
					|| !pivot.CCA_MiscID.IsEmpty
					|| pivot.CFIARegistrationNumbers.Any());
		}

		bool ShouldTransformForTires(CusClassPartPivot pivot)
		{
			return pivot.CCA_TCIndicator.IsEmpty && !TCIntendedUseCodes.ConvertFromOGDCode(pivot.CCA_ImportReasonCode).IsEmpty && CARefTariffDataLoader.DoesTariffHasPGAType(pivot.Factory, pivot.TariffNumber, PGACodes.Codes.TC, ZDateTime.Today);
		}

		void CopyCFIAOGDToPGA(CusClassPartPivot pivot)
		{
			if (ShouldTransformForCFIA(pivot))
			{
				pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
				pivot.CFIAPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
				pivot.CFIAPGAHeader.CA_AIRSExtensionCode = pivot.CCA_AirsCode;
				pivot.CFIAPGAHeader.CA_AIRSEndUse = pivot.CCA_EndUse;
				pivot.CFIAPGAHeader.CA_AIRSMiscellaneous = pivot.CCA_MiscID;

				foreach (CFIARegistrationNumber registrationNumber in pivot.CFIARegistrationNumbers)
				{
					var lpco = pivot.CFIAPGAHeader.LPCOViews.AddNew();
					lpco.CLP_Type = registrationNumber.CY_Code.Left(AutoCusCALPCO.Schema.CLP_TypeMaxLength);
					lpco.CLP_RefNo = registrationNumber.CY_Data.Left(AutoCusCALPCO.Schema.CLP_RefNoMaxLength);
				}
			}
		}

		void CopyTiresOGDToPGA(CusClassPartPivot pivot)
		{
			if (ShouldTransformForTires(pivot))
			{
				pivot.CCA_TCIndicator = YesNoList.Codes.Yes;
				pivot.TCPGAHeader.CA_TPRProgramInd = YesNoList.Codes.Yes;
				pivot.TCPGAHeader.CA_ImportReasonCode = TCIntendedUseCodes.ConvertFromOGDCode(pivot.CCA_ImportReasonCode);
			}
		}

		#endregion

		#region GetIndicationForCopyingOGDToPGAData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static Tuple<ZString, ZBool> GetIndicationForCopyingOGDToPGAData()
		{
			var registryValue = new ZDateTime(CACustomsDataRegistry.Instance.CopyOGDToPGADataDateTime.Value);
			var factory = new BusinessObjectFactory();
			string indicationMessage;

			if (registryValue.IsValid)
			{
				var productsCount = GetProductsNeedTransformCount(factory);
				if (productsCount > 0)
				{
					indicationMessage = Res.GetString("95c232fa-258e-4873-8226-eafd17fb8ccb",
						"The transformation was triggered at {0} UTC. {1} product(s) need to be processed. Please wait until they are finished before triggering another cycle of transformation. This will be processed in the background by the CCP service task."
						, registryValue, productsCount);

					return new Tuple<ZString, ZBool>(indicationMessage, false);
				}

				SetMoveOGDToPGADataDateTimeToEmpty();
			}

			try
			{
				var sql = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO dbo.GenAddOnColumn ([XA_PK],[XA_Name],[XA_ParentID],[XA_ParentTableCode])
SELECT NEWID(), '{0}', P.CI_OP, 'OP' FROM
(
	SELECT DISTINCT CI_OP FROM dbo.CusClassPartPivot
	LEFT JOIN dbo.CusCAClassification ON CI_PK = CCA_ParentID AND CCA_ParentTableCode = 'CI'
	WHERE
		CI_RN_NKCountry='CA'
		AND 
		(
			(
				ISNULL(CCA_CFIAIndicator, '') = ''
				AND (ISNULL(CCA_AirsCode, '') != '' OR ISNULL(CCA_EndUse, '') != ''  OR ISNULL(CCA_MiscID, '') != '' OR CI_PK in (SELECT CY_ParentID FROM dbo.CusCodeData WHERE CY_ParentTableCode='CI' and CY_Type='RNF'))
			)
			OR
			(
				ISNULL(CCA_TCIndicator, '') = '' AND ISNULL(CCA_ImportReasonCode, '') IN ('01', '02', '04', '06')
			)
		)
		AND CI_OP NOT IN
		(
			SELECT XA_ParentID from dbo.GenAddOnColumn WHERE XA_Name = '{0}'
		)
) P", GenAddOnColumnCachedName);

				var lines = Db.Connection.ExecuteNonQuery(sql);
				if (lines > 0)
				{
					CACustomsDataRegistry.Instance.CopyOGDToPGADataDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());

					indicationMessage = Res.GetString("24716d4c-76f2-4dc4-8cc2-67e2f2eb96fa", "A new transformation cycle has be successfully triggered. {0} product(s) registered to be transformed. This will be processed in the background by the CCP service task.", lines);
					return new Tuple<ZString, ZBool>(indicationMessage, true);
				}

				indicationMessage = Res.GetString("12f84df3-d968-4a5b-964a-e7bb41b94fde", "There is no product to be transformed.");
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce(Res.GetString("acc9d521-3333-4d07-b49f-f291b375460f", "Copy OGD to PGA data is broken."), e);
				indicationMessage = string.Empty;
			}

			return new Tuple<ZString, ZBool>(indicationMessage, false);
		}

		public static void SetMoveOGDToPGADataDateTimeToEmpty()
		{
			var updateSql = "Update dbo.StmData set SD_BinaryValue = null where SD_Name = '{0}'";
			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, updateSql, CACustomsDataRegistry.Instance.CopyOGDToPGADataDateTime.Name));
		}

		static int GetProductsNeedTransformCount(BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(GenAddOnColumn));
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, GenAddOnColumnCachedName);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, OrgSupplierPartSchema.Constants.Prefix);
			return factory.GetDatabaseCount(typeof(GenAddOnColumn), query);
		}

		#endregion
	}
}
