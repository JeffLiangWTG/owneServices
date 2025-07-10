using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TIP
{
	public class ProductExporter
	{
		public ProductExporter(INotifications notify)
		{
			this.Notify = notify;
			FactoryProvider = new BusinessObjectFactoryProvider();
		}

		public void Export(CancellationToken token)
		{
			ExportCore(token);
		}

		#region Implementation

		internal void Export()
		{
			ExportCore(CancellationToken.None);
		}

		protected virtual
		void ExportCore(CancellationToken token)
		{
			DynamicBusinessObjectCollection partCollection = new DynamicBusinessObjectCollection(FactoryProvider.Current);
			partCollection.Load(SqlScript, GetAllParameters());

			if (partCollection.Count > 0)
			{
				OrgPartRelationRegistryBusinessObjectCollection orgCollection = TIPDataRegistry.Instance.OrganisationProductRegistryItem.Value;
				Dictionary<OrgPartRelationRegistryBusinessObject, List<ZGuid>> orgList = new Dictionary<OrgPartRelationRegistryBusinessObject, List<ZGuid>>();

				foreach (OrgPartRelationRegistryBusinessObject orgSetting in orgCollection)
				{
					token.ThrowIfCancellationRequested();
					for (int i = 0; i < partCollection.Count; i++)
					{
						DynamicBusinessObject bizO = partCollection[i];
						AUOrgSupplierPart part = FactoryProvider.Current.Load<AUOrgSupplierPart>((ZGuid)bizO[OrgSupplierPartSchema.PK]);

						if (part != null)
						{
							var orgPartRelation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgSetting.OrgHeaderPK, orgSetting.RelationshipType);

							if (orgPartRelation != null)
							{
								List<ZGuid> partList = null;
								orgList.TryGetValue(orgSetting, out partList);
								if (partList == null)
								{
									partList = new List<ZGuid>();
									orgList.Add(orgSetting, partList);
								}
								partList.Add(part.PK);

								Notify.Notify(new InfoNotification($"Product '{part.OP_PartNum}': [Organisation ='{orgPartRelation.Organisation.OH_Code}', Relationship ='{orgPartRelation.OU_Relationship}'] is shortlisted for export."));
							}
						}
					}
				}

				foreach (KeyValuePair<OrgPartRelationRegistryBusinessObject, List<ZGuid>> pair in orgList)
				{
					token.ThrowIfCancellationRequested();
					var org = pair.Key;
					var partPKs = pair.Value;
					ExportProductsForOrganisation(org, partPKs);
				}
			}
		}

		void ExportProductsForOrganisation(OrgPartRelationRegistryBusinessObject orgRelation, List<ZGuid> partPKs)
		{
			OrgHeader org = FactoryProvider.Current.Load<OrgHeader>(orgRelation.OrgHeaderPK);
			ZString fileName = org.OH_Code + "_" + orgRelation.RelationshipType + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
			ZString pathToFile = Path.Combine(TIPDataRegistry.Instance.ProductExportDirectory, fileName);

			Notify.Notify(new InfoNotification($"Generating Product Records for Organisation ='{orgRelation.Organisation.OH_Code}' Relationship ='{orgRelation.RelationshipType}'."));
			Notify.Notify(new InfoNotification($"Estimated # of Products: {partPKs.Count}. \r\nOutput File Name: {fileName}"));

			try
			{
				using (StreamWriter writer = new StreamWriter(pathToFile, true))
				{
					int partCount = 0;
					foreach (ZGuid partPK in partPKs)
					{
						AUOrgSupplierPart part = FactoryProvider.Current.Load<AUOrgSupplierPart>(partPK);
						if (part != null && !part.IsDeleted)
						{
							bool isExport = (orgRelation.RelationshipType == OrgPartRelation.RelationshipTypes.Supplier);
							ZString productLine = Wrapper.GetProductLine(part, isExport, orgRelation.PK, Notify);
							if (!productLine.IsEmpty)
							{
								writer.WriteLine(productLine);
								part.Logs.AddNew(Events.DataExport, TIPConstants.ProductDEXReference);
								partCount++;
							}
							if (partCount > 100)
							{
								FactoryProvider.SaveCurrentAndCreateNew();
								partCount = 0;
							}
						}
						else
						{
							Notify.Notify(new InfoNotification($"Product with PK '{partPK}' is either not found or is deleted at the time of export."));
						}
					}
					writer.Close();
					FactoryProvider.SaveCurrentAndCreateNew();
				}
				if (File.Exists(pathToFile) && new FileInfo(pathToFile).Length == 0)
				{
					File.Delete(pathToFile);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
		}

		ProductWrapper Wrapper
		{
			get { return wrapper ?? (wrapper = new ProductWrapper()); }
		}
		ProductWrapper wrapper;

		ZSqlParameterCollection OrgParameters
		{
			get
			{
				if (orgParameters == null)
				{
					orgParameters = new ZSqlParameterCollection();
					OrgPartRelationRegistryBusinessObjectCollection orgs = TIPDataRegistry.Instance.OrganisationProductRegistryItem.Value;
					foreach (OrgPartRelationRegistryBusinessObject org in orgs)
					{
						orgParameters.Add("@" + org.OrgHeaderPK.ToString().Replace("-", ""), org.OrgHeaderPK, OrgPartRelationSchema.OU_OH);
					}
				}
				return orgParameters;
			}
		}
		ZSqlParameterCollection orgParameters;

		ZSqlParameterCollection GetAllParameters()
		{
			ZSqlParameterCollection result = new ZSqlParameterCollection();
			result.AddRange(OrgParameters);
			ZSqlParameter lastRunParam = ZSqlParameter.New("@HighWaterMark", TIPDataRegistry.Instance.HighWaterMark, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan);
			ZSqlParameter addEventParam = ZSqlParameter.New("@AddEventCode", Events.AddedARecordToTheSystem.Code, StmALogSchema.SL_SE_NKEvent);
			ZSqlParameter edtEventParam = ZSqlParameter.New("@EdtEventCode", Events.EditedARecord.Code, StmALogSchema.SL_SE_NKEvent);
			result.Add(lastRunParam);
			result.Add(addEventParam);
			result.Add(edtEventParam);
			return result;
		}

		string SqlScript
		{
			get
			{
				ZStringBuilder orgParamList = new ZStringBuilder();
				foreach (ZSqlParameter param in OrgParameters)
				{
					orgParamList.Append(param.ParameterName);
				}

				return "select OP_PK " +
				  "from dbo.OrgSupplierPart " +
				 "where OP_PK in " +
						"( select SL_Parent from dbo.StmALog " +
						   "where SL_PostedTimeUtc >= @HighWaterMark" +
							" and SL_SE_NKEvent in (@AddEventCode, @EdtEventCode)" +
						") " +
				   "and OP_PK in " +
						"( select OU_OP " +
							"from dbo.OrgPartRelation " +
						   "where OU_OH in (" +
							orgParamList.ToStringWithDelimiterBetweenAppends(",") +
						")" +
					"and OP_PK in (select CI_OP from dbo.CusClassPartPivot)" +
				")";
			}
		}

		readonly BusinessObjectFactoryProvider FactoryProvider;
		readonly INotifications Notify;

		#endregion
	}
}
