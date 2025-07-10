using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACMessageBuilder : BaseImportMessageBuilder
	{
		public SACMessageBuilder(CusEntryHeader entryHeader, CMRMessageTypes messageType)
			: base(entryHeader, messageType)
		{
		}

		#region Header Section

		protected internal override void PopulateLocations()
		{
			base.PopulateLocations();

			JobComInvoiceHeader invoice = EntryHeader.RandomHeader;
			if (IsSACWithLines && invoice != null && !invoice.AddInfo.ZA_ORG.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				MessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.CountryOfOrigin, invoice.AddInfo.ZA_ORG, CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization);
			}

			if (!EntryHeader.Declaration.AddInfo.ZA_AQISInspectLocation_Hidden.IsEmpty)
			{
				LOCSegment lOC = cUSDEC.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceLocationWhereSpecialTreatmentsHaveHappenedOrMustHappen;
				lOC.LocationIdentification.LocationName = EntryHeader.Declaration.AddInfo.ZA_AQISInspectLocation_Hidden;
			}
		}

		protected internal override void PopulateGIS()
		{
			base.PopulateGIS();

			//Official receipt request
			PopulateGIS(MessageType != CMRMessageTypes.PreLodge && Env.Registry.RequestOfficialCustomsPaymentReceipt, "POR");
		}

		#region FTX

		protected internal override void PopulateFTX()
		{
			base.PopulateFTX();
			PopulateFTX(TextSubjectCodeQualifierList.GoodsDescription, Declaration.JE_GoodsDescription);
		}

		protected internal override void PopulateQuestions()
		{
			base.PopulateQuestions();
			GenericPopulateQuestions("THIS IS THE REFERAL REASON");
		}

		#endregion

		#region Questions

		protected override void PopulateWithdrawalQuestions()
		{
			base.PopulateWithdrawalQuestions();
			GenericPopulateQuestions(ZString.Empty);
		}

		protected void GenericPopulateQuestions(ZString freeText3)
		{
			if (EntryHeader != null)
			{
				EntryHeader.Questions.Sort(CusEntryCPDecSchema.ON_CPDecNum.Name, System.ComponentModel.ListSortDirection.Ascending);
				foreach (CMRCusEntryCPDec currentQuestion in EntryHeader.Questions)
				{
					ZString questionId = currentQuestion.ON_CPDecNum.ToString().PadLeft(4, '0');
					if (currentQuestion.IsAnswered)
					{
						if (!currentQuestion.IsAcknowledge)
						{
							PopulateFTX(TextSubjectCodeQualifierList.Reason, questionId, currentQuestion.ON_AnswerCode, freeText3);
						}
						else if (IsWithdrawal && currentQuestion.IsAcknowledge && currentQuestion.IsYes)
						{
							PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ProfileNumber, questionId);
						}
					}
				}
			}
		}

		#endregion

		protected internal override void PopulateDTM()
		{
			base.PopulateDTM();
			if (IsSACWithLines)
			{
				PopulateDTM(DateTimePeriodFunctionCodeQualifierList.ValuationDateCustoms, EntryHeader.EffectiveValuationDate);
			}
		}

		#endregion

		#region Segment Group 1

		protected internal override void PopulateGroup1()
		{
			base.PopulateGroup1();

			if (IsAmendment)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber, EntryHeader.EntryNumber);
			}
			foreach (CusContainer currentContainer in Declaration.CusContainers)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, currentContainer.CO_ContainerNumber);

				SegmentGroup2 group2 = Group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
				PACSegment pAC = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pAC.PackageType.PackageTypeDescriptionCode = currentContainer.CO_FCL_LCL_AIR_ForMessaging;
				pAC.PackageType.CodeListIdentificationCode = CodeListIdentificationCodeList.TypeOfPackage;
				pAC.PackageType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}

			ReferenceFunctionCodeQualifierList houseBillCode = Declaration.IsSea ? ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber : ReferenceFunctionCodeQualifierList.HouseWaybillNumber;
			ReferenceFunctionCodeQualifierList masterBillCode = Declaration.IsSea ? ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber : ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber;

			foreach (Customs.Business.Bill bill in Declaration.LowestBills)
			{
				ReferenceFunctionCodeQualifierList qualifier = bill.IsHouseBill ? houseBillCode : masterBillCode;
				PopulateRFF(Group1, qualifier, bill.CU_BillNum);
				if (!bill.IsMasterBill)
				{
					ZString masterBill = bill.CU_MasterBill;
					if (!masterBill.IsEmpty)
					{
						PopulateRFF(Group1, masterBillCode, masterBill);
					}
				}
			}

			if (Declaration.IsAir)
			{
				PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.GetFromString("CNR"), Declaration.JE_PartShipConsignmentReference);
			}

			EntryHeader.Questions.Sort(CusEntryCPDecSchema.ON_CPDecNum.Name, System.ComponentModel.ListSortDirection.Ascending);
			foreach (CMRCusEntryCPDec question in EntryHeader.Questions)
			{
				if (question.IsAcknowledge && question.IsYes)
				{
					PopulateRFF(Group1, ReferenceFunctionCodeQualifierList.ProfileNumber, question.ON_CPDecNum.ToString().PadLeft(4, '0'));
				}
			}
		}

		#endregion

		#region Segment Group 4

		protected override void PopulateAirDetails()
		{
			TDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			TDT.ModeOfTransport.TransportModeNameCode = "A";
		}

		#endregion

		#region Segment Group 6

		protected internal override void PopulateNADForImporter(OrgHeader importer)
		{
			base.PopulateNADForImporter(importer);
			if (importer != null && importer.LocalBusinessRegNo.IsEmpty && importer.GetCustomsClientID().IsEmpty)
			{
				OrgAddress importerAddress = importer.MainAddress;
				if (importerAddress != null)
				{
					var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
					NADSegment nAD = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
					nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Importer;

					nAD.NameAndAddress.NameAndAddressLine1 = importerAddress.OA_City.SubstringSafe(0, 35);
					nAD.NameAndAddress.NameAndAddressLine2 = importerAddress.OA_City.SubstringSafe(35);
					nAD.PartyName.PartyName1 = importer.OH_FullName.SubstringSafe(0, 35);
					nAD.PartyName.PartyName2 = importer.OH_FullName.SubstringSafe(35, 70);
					nAD.Street.StreetAndNumberPOBox1 = importerAddress.OA_Address1.SubstringSafe(0, 35);
					nAD.Street.StreetAndNumberPOBox2 = importerAddress.OA_Address1.SubstringSafe(35, 70);
					nAD.Street.StreetAndNumberPOBox3 = importerAddress.OA_Address1.SubstringSafe(70);
					nAD.CountrySubEntityDetails.CountrySubEntityName = importerAddress.OA_State.SubstringSafe(0, 35);
					nAD.PostalIdentificationCode = importerAddress.OA_PostCode.SubstringSafe(0, 17);
					nAD.CountryNameCode = importerAddress.OA_RL_NKRelatedPortCode.SubstringSafe(0, 2);
				}
			}
		}

		protected internal override void PopulateGroup6()
		{
			base.PopulateGroup6();

			CargoReportHelper.DoICSRelease(() =>
			{
				var vendor = CargoHelper.GetConsignorVendor(Declaration.Supplier);
				if (!vendor.IsEmpty)
				{
					var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
					MessageUtilities.PopulateNAD(group6.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.Vendor, vendor, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}

				ZString supplierTIN = CargoHelper.GetTraderIdentificationNumber(Declaration.Supplier);
				if (!supplierTIN.IsEmpty)
				{
					var group6 = cUSDEC.Group6.InstantiateAChildAndAddItToChildrenCollection();
					MessageUtilities.PopulateNAD(group6.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.AuthorizedTraderTransit, supplierTIN, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
				}
			});
		}

		protected override SegmentGroup6 PopulateLocalCustomsBranchIdentifierIfSpecified()
		{
			var group6 = base.PopulateLocalCustomsBranchIdentifierIfSpecified();

			var localContactPhoneNumber = AUCustomsDataRegistry.Instance.LocalContactPhoneNumber.Value;
			if (string.IsNullOrEmpty(Env.Registry.AUCustoms.LocalCustomsBranchIdentifier) &&
				!string.IsNullOrEmpty(localContactPhoneNumber))
			{
				var cta = group6.CTA.InstantiateAChildAndAddItToChildrenCollection();
				cta.ContactFunctionCode = ContactFunctionCodeList.InformationContact;

				var com = group6.COM.InstantiateAChildAndAddItToChildrenCollection();
				com.CommunicationContact.CommunicationNumberCodeQualifier = CommunicationNumberCodeQualifierList.Telephone;
				com.CommunicationContact.CommunicationNumber = localContactPhoneNumber;
			}

			return group6;
		}

		#endregion

		#region Segment Group 8

		protected internal override void PopulateGroup8()
		{
			if (IsSACWithLines)
			{
				PopulateMOA(Group8, EntryHeader.TransportAndInsuranceInLocalCurrency, MonetaryAmountTypeCodeQualifierList.InsuranceAndTransportChargesCustoms);
				PopulateMOA(Group8, EntryHeader.CustomsValueInAUD, MonetaryAmountTypeCodeQualifierList.CustomsValue);
			}
		}

		#endregion

		#region Segment Group 30

		protected internal override void PopulateGroup30()
		{
			if (IsSACWithLines)
			{
				foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
				{
					new SACMessageLine(entryLine, cUSDEC.Group30.InstantiateAChildAndAddItToChildrenCollection()).Populate(entryLine.CL_LineNumber, LineAction.Insert);
				}
			}
		}

		#endregion

		#region Implementation

		bool IsSACWithLines => EntryHeader.Declaration.IsSACWithLines;

		protected internal override ZString DocumentName => "SAC";

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.GoodsDeclarationForImportation;

		protected internal override ZString EM_MessageType => CMRMessage.CMRMessageTypes.SAC;

		protected internal override Type TypeOfMessage => typeof(CMRSACMessage);

		#endregion
	}
}
