using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryInstruction = Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction;
using CusInBondMoveHeader = Enterprise.Customs.AsycudaCustoms.Business.CusInBondMoveHeader;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : Customs.DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null) : base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override void ImportCountrySpecificRelatedData(Customs.Business.BaseJobDeclaration declaration)
		{
			base.ImportCountrySpecificRelatedData(declaration);
			if (dataObject.ManifestNumber.HasValue)
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_ManifestNumber, dataObject.ManifestNumber.Value);
			}

			FillInBondMoveHeaderCollection((JobDeclaration)declaration, dataObject);
		}

		void FillInBondMoveHeaderCollection(JobDeclaration declaration, Shipment declarationData)
		{
			if (declarationData.InBondMoveHeaderCollection != null)
			{
				var customsCountryCode = declaration.CountryCode;
				foreach (var inBondMoveHeadersData in declarationData.InBondMoveHeaderCollection.GroupBy(x => GetEntryInstruction(declaration, x)))
				{
					if (inBondMoveHeadersData.Key is CusEntryInstruction entryInstruction)
					{
						var existingPermitHeaders = entryInstruction.CusInBondPermitsHeaders.ToList();

						foreach (var inBondMoveHeaderData in inBondMoveHeadersData)
						{
							var entryNumberData = GetEntryNumberData(inBondMoveHeaderData, customsCountryCode);
							var permitNumber = entryNumberData?.Number.GetValueOrDefault() ?? ZString.Empty;
							var permit = FindExistingPermitOrCreateNewOne(entryInstruction, existingPermitHeaders, permitNumber);
							SetValue(permit, CusInBondMoveHeaderSchema.BM_AdditionalText, inBondMoveHeaderData.AdditionalText);

							if (entryNumberData != null)
							{
								var entryNum = permit.PermitNumberBizObj;
								SetValue(entryNum, CusEntryNumSchema.CE_IssueDate, entryNumberData.IssueDate);
								SetValue(entryNum, CusEntryNumSchema.CE_ExpiryDate, entryNumberData.ExpiryDate);
							}

							if (inBondMoveHeaderData.DateCollection != null)
							{
								var arrivalDateData = inBondMoveHeaderData.DateCollection.FirstOrDefault(x => x.Type == DateType.Arrival);
								if (arrivalDateData != null)
								{
									SetValue(permit, CusInBondMoveHeaderSchema.BM_ArrivalDate, arrivalDateData.Value);
								}
							}
						}
						existingPermitHeaders.DeleteAll();
					}
				}
			}
		}

		CusEntryInstruction GetEntryInstruction(JobDeclaration declaration, InBondMoveHeader inBondMoveHeaderData)
		{
			if (inBondMoveHeaderData.AdditionalReferenceCollection != null)
			{
				var entryInstructionLinkReference = inBondMoveHeaderData.AdditionalReferenceCollection.FirstOrDefault(x => x.ContextInformation.GetValueOrDefault() == UniversalCustomsDataConstants.EntryInstructionLinkContext && x.Type.GetCodeAsUpperCase() == UniversalCustomsDataConstants.EntryInstructionLinkType);
				if (entryInstructionLinkReference != null)
				{
					if (ZInt.TryParse(entryInstructionLinkReference.ReferenceNumber, out var entryInstructionLink))
					{
						var entryInstructionPK = Helper.GetEntryInstructionPK(entryInstructionLink);
						if (entryInstructionPK.HasValue)
						{
							var entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault<CusEntryInstruction>(x => x.PK == entryInstructionPK);
							if (entryInstruction != null)
							{
								return entryInstruction;
							}
						}
					}
				}
			}

			return null;
		}

		CusInBondMoveHeader FindExistingPermitOrCreateNewOne(CusEntryInstruction entryInstruction, List<CusInBondMoveHeader> existingPermitHeaders, ZString permitNumber)
		{
			var permit = existingPermitHeaders.FirstOrDefault(x => x.BM_Calc_PermitNumber == permitNumber);
			if (permit == null)
			{
				permit = entryInstruction.CusInBondPermitsHeaders.AddNew();
				SetValue(permit.PermitNumberBizObj, CusEntryNumSchema.CE_EntryNum, permitNumber);
			}
			else
			{
				existingPermitHeaders.Remove(permit);
			}
			return permit;
		}

		static EntryNumber GetEntryNumberData(InBondMoveHeader inBondMoveHeaderData, ZString customsCountryCode)
		{
			return inBondMoveHeaderData.EntryNumberCollection?.FirstOrDefault(x => x.Type != null && x.Type.Code.GetValueOrDefault() == UniversalCustomsDataConstants.PermitNumberType && x.CountryOfIssue.GetCodeAsUpperCase() == customsCountryCode);
		}
	}
}
