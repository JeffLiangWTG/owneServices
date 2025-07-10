using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : Customs.DataTransfer.Universal.DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override void PopulateCountrySpecificData(Shipment declarationData, Customs.Business.BaseJobDeclaration declarationBO)
		{
			base.PopulateCountrySpecificData(declarationData, declarationBO);

			declarationData.ManifestNumber = declarationBO.JE_ManifestNumber;
			PopulateInBondMoveHeaderCollection(declarationData, (JobDeclaration)declarationBO);
		}

		void PopulateInBondMoveHeaderCollection(Shipment declarationData, JobDeclaration declaration)
		{
			var inBondMoveHeaderDataCollection = new List<InBondMoveHeader>();
			foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
			{
				var entryInstructionLink = helper.GetAllocatedEntryInstructionLink(entryInstruction.PK);
				if (entryInstructionLink.HasValue)
				{
					foreach (var inBondMoveHeader in entryInstruction.CusInBondPermitsHeaders.Cast<CusInBondMoveHeader>())
					{
						var inBondMoveHeaderData = new InBondMoveHeader(writeManager.WriterStrategy);
						inBondMoveHeaderData.AdditionalText = inBondMoveHeader.BM_AdditionalText;
						inBondMoveHeaderData.SetEntryNumberCollection(() => new List<EntryNumber>
						{
							new EntryNumber
							{
								Number = inBondMoveHeader.BM_Calc_PermitNumber,
								Type = new EntryType
								{
									Code = UniversalCustomsDataConstants.PermitNumberType,
									Description = UniversalCustomsDataConstants.PermitNumberDescription
								},
								CountryOfIssue = Country.New(inBondMoveHeader.PermitNumberBizObj.Country),
								ExpiryDate = inBondMoveHeader.BM_Calc_ValidityDate,
								IssueDate = inBondMoveHeader.BM_Calc_IssueDate
							}
						});

						if (!inBondMoveHeader.BM_ArrivalDate.IsEmpty)
						{
							inBondMoveHeaderData.SetDateCollection(() => new List<Date>
							{
								new Date
								{
									Type = DateType.Arrival,
									Value = inBondMoveHeader.BM_ArrivalDate,
									IsEstimate = false
								}
							});
						}

						inBondMoveHeaderData.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = UniversalCustomsDataConstants.EntryInstructionLinkType,
									Description = UniversalCustomsDataConstants.EntryInstructionLinkDescription
								},
								ContextInformation = UniversalCustomsDataConstants.EntryInstructionLinkContext,
								ReferenceNumber = entryInstructionLink.Value.ToString()
							}
						});

						inBondMoveHeaderDataCollection.Add(inBondMoveHeaderData);
					}
				}
			}
			declarationData.SetInBondMoveHeaderCollection(() => inBondMoveHeaderDataCollection);
		}
	}
}
