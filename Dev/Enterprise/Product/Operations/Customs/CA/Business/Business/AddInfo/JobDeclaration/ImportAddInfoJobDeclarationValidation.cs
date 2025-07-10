using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business
{
	public class ImportAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public ImportAddInfoJobDeclarationValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		JobDeclaration Declaration
		{
			get { return Parent.Parent; }
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return Declaration.DeclarationValidator; }
		}

		#region CheckCA_AnySightDepositAmount (validate if entered)

		protected override void CheckCA_AnySightDepositAmount()
		{
			base.CheckCA_AnySightDepositAmount();
			var declaration = Declaration;
			if (declaration.CA_AnySightDepositAmount.IsEmpty)
			{
				if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.CashD)
				{
					declaration.CA_AnySightDepositAmountInfo.AddMessageError(Res.GetString("ba29f1a4-6ce7-4214-9ee2-a88f50871559", "Deposit amounts are mandatory on Sight Type D Jobs."));
				}
				else if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.ConfirmingSight)
				{
					declaration.CA_AnySightDepositAmountInfo.AddMessageError(Res.GetString("31163025-8b8d-46ab-97b4-4874ab16a853", "Deposit amounts are mandatory on Confirming Sight Type AD Jobs."));
				}
			}
		}

		#endregion

		#region CheckCA_PriorityInd (ACROSS)

		protected override void CheckCA_PriorityInd()
		{
			base.CheckCA_PriorityInd();
			DeclarationValidator.MessageErrorIfInvalidCode(Parent.CA_PriorityIndInfo, Parent.Lookups.PriorityIndicatorList, ValidateForMessageType.ACROSS);
		}

		#endregion

		#region CheckCA_UnladingOffice (validate if entered)

		protected override void CheckCA_UnladingOffice()
		{
			var declaration = Declaration;
			if (declaration.ShowShipmentRelatedFieldsOrSea)
			{
				base.CheckCA_UnladingOffice();
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_UnladingOfficeInfo, Lookups.CBSAOffices);

				if (!declaration.IsLVS && declaration.IsSea && declaration.HasUSPlaceOfExportInvoice)
				{
					DeclarationValidator.MessageErrorIfNotEntered(Parent.CA_UnladingOfficeInfo, string.Empty, ValidateForMessageType.B3CUSDEC);
				}
			}
		}

		#endregion

		#region CheckCA_SubLocationName (ACROSS)

		protected override void CheckCA_SubLocationName()
		{
			base.CheckCA_SubLocationName();
			Declaration.Validation.ValidateJE_LocationOfGoods();

			if (Parent.CA_SubLocationName.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.CA_SubLocationNameInfo.AddWarning(Res.GetString("7b279ee2-c4ed-4e08-9e8f-cc5debd3b40b", "The Sub-Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}
		}

		#endregion

		#region CA_ExamLocation

		protected override void CheckCA_ExamLocationCode()
		{
			base.CheckCA_ExamLocationCode();
			var declaration = Declaration;
			if (!declaration.IsLVS)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_ExamLocationCodeInfo, (IBusinessObjectCollection)declaration.Lookups.ExamLocationCodes);
				if (declaration.IsIID && (Parent.CA_ExamLocationCode.IsEmpty && Parent.CA_ExamLocationName.IsEmpty))
				{
					Parent.CA_ExamLocationCodeInfo.AddMessageError(ExamLocationMustBeEntered);
				}
				if (!Parent.CA_ExamLocationCode.IsEmpty && !Parent.CA_ExamLocationCodeInfo.HasNotifications())
				{
					var subLocation = CACSubLocation.Load(Parent.Factory, Parent.CA_ExamLocationCode);
					if (subLocation != null && subLocation.SL_Port.TrimStart('0') != declaration.JE_CustomsOffice.TrimStart('0'))
					{
						Parent.CA_ExamLocationCodeInfo.AddMessageError(Res.GetString("D73FD880-C574-4a14-8BD0-A7E580FC8323", "The Customs Port of Clearance is not valid for this exam location. Either the exam location code is incorrect or the Port of Clearance should be {0}", subLocation.SL_Port.TrimStart('0')));
					}
				}
			}
		}

		internal static string ExamLocationMustBeEntered
		{
			get
			{
				return Res.GetString("482751FC-3CAD-4e75-BADF-83BF1F3EB020", "You must enter either a exam location code or a text description.");
			}
		}

		#endregion

		#region CheckCA_ServiceOption (ACROSS)

		protected override void CheckCA_ServiceOption()
		{
			base.CheckCA_ServiceOption();
			DeclarationValidator.MessageErrorIfInvalidCode(Parent.CA_ServiceOptionInfo, Parent.Lookups.ServiceOptions, ValidateForMessageType.ACROSS);
			ValidateACROSSOptions();
			var declaration = Declaration;
			if (!declaration.IsIID && declaration.ClassificationsHasPGARequirements)
			{
				Parent.CA_ServiceOptionInfo.AddMessageError(ClassificationsRequirePGAs);
			}
		}

		internal static string ClassificationsRequirePGAs
		{
			get
			{
				return Res.GetString("b0ee1443-d03f-4bcc-b79d-345ec2237342", "At least one classification line requires PGA filing, please consider changing Release Service Option to IID");
			}
		}

		void ValidateACROSSOptions()
		{
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
			{
				var validator = new EDIReleaseValidator(Declaration);
				if (!validator.AllOptionsSpecifiedAndValid() || !validator.IsCombinationValid())
				{
					Parent.CA_ServiceOptionInfo.AddMessageError(validator.LastErrorMessage);
				}
			}
		}

		#endregion

		#region CheckCA_AssesmentOption (ACROSS)

		protected override void CheckCA_AssesmentOption()
		{
			base.CheckCA_AssesmentOption();
			DeclarationValidator.MessageErrorIfInvalidCode(Parent.CA_AssesmentOptionInfo, Parent.Lookups.AssessmentOptions, ValidateForMessageType.ACROSS);
			if (!Parent.CA_AssesmentOptionInfo.HasMessageErrors())
			{
				ValidateCA_ServiceOption();
			}
		}

		#endregion

		#region CheckCA_NetWeight (common)

		protected override void CheckCA_NetWeight()
		{
			base.CheckCA_NetWeight();
			CompareValidation.CheckNumberNotNegative(Parent.CA_NetWeightInfo);
		}

		protected override void CheckCA_NetWeightUQ()
		{
			base.CheckCA_NetWeightUQ();
			if (Parent.CA_NetWeight > 0 && Parent.CA_NetWeightUQ.IsEmpty)
			{
				Parent.CA_NetWeightUQInfo.AddMessageError(Res.GetString("7C231650-F890-433B-8FCC-B1F412DA2EE0", "Net weight unit must be entered."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_NetWeightUQInfo, Declaration.Lookups.WeightUnitList);
			}
		}

		#endregion

		#region Check OGD indicators (ACROSS)

		protected override void CheckCA_OGDCFIA()
		{
			base.CheckCA_OGDCFIA();
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.CA_OGDCFIA && !Declaration.IsOGD)
			{
				Parent.CA_OGDCFIAInfo.AddMessageError(notAnOGDSO);
			}
		}

		protected override void CheckCA_OGDIC()
		{
			base.CheckCA_OGDIC();
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.CA_OGDIC && !Declaration.IsOGD)
			{
				Parent.CA_OGDICInfo.AddMessageError(notAnOGDSO);
			}
		}

		protected override void CheckCA_OGDNR()
		{
			base.CheckCA_OGDNR();
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.CA_OGDNR && !Declaration.IsOGD)
			{
				Parent.CA_OGDNRInfo.AddMessageError(notAnOGDSO);
			}
		}

		protected override void CheckCA_OGDTC()
		{
			base.CheckCA_OGDTC();
			if (DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS) && Parent.CA_OGDTC && !Declaration.IsOGD)
			{
				Parent.CA_OGDTCInfo.AddMessageError(notAnOGDSO);
			}
		}

		internal static string notAnOGDSO
		{
			get { return Res.GetString("B4ABB426-26D6-47F5-AE55-1E705BAB5459", "You have not selected either Service Option 463 or 471 on the declaration Tab"); }
		}

		#endregion

		#region CheckCA_ATDExCode (validate if entered)

		protected override void CheckCA_ATDExCode()
		{
			base.CheckCA_ATDExCode();
			ListValidation.ErrorIfInvalidCode(Parent.CA_ATDExCodeInfo, Parent.Lookups.ATDExemptionCodes);
		}

		#endregion

		#region CheckCA_AmendReasonCode (validate if entered)

		protected override void CheckCA_AmendReasonCode()
		{
			base.CheckCA_AmendReasonCode();
			if (Declaration.IsIID)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_AmendReasonCodeInfo, Parent.Lookups.AmendmentCodes);
			}
		}

		#endregion

		#region CheckCA_ProvinceOfClearance

		protected override void CheckCA_ProvinceOfClearance()
		{
			base.CheckCA_ProvinceOfClearance();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_ProvinceOfClearanceInfo, Parent.Lookups.CanadianProvinces);
		}

		#endregion

		#region CheckCA_MergeBy

		protected override void CheckCA_MergeBy()
		{
			base.CheckCA_MergeBy();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_MergeByInfo, Lookups.CAMergeByList);
		}

		#endregion

		#region CheckCA_BondType

		protected override void CheckCA_BondType()
		{
			base.CheckCA_BondType();
			var declaration = Declaration;
			if (declaration.CA_BondType.IsEmpty)
			{
				declaration.CA_BondTypeInfo.AddMessageError(ImporterIsNotOnCARMPortal);
			}
			else
			{
				ValidateBondType(declaration, declaration.CA_BondTypeInfo, declaration.CA_BondType);
			}
		}

		internal static void ValidateBondType(JobDeclaration declaration, ZPropertyInfo notificationInfo, ZString bondType)
		{
			if (!bondType.IsEmpty)
			{
				if (bondType == BondTypeList.Codes.NotOnPortal)
				{
					notificationInfo.AddMessageError(ImporterIsNotOnCARMPortal);
				}
				else if (bondType == BondTypeList.Codes.OnPortal)
				{
					notificationInfo.AddWarning(ClinetIsInCARMPortalButNoBondOnFile);
				}
			}
		}

		internal static string ImporterIsNotOnCARMPortal
		{
			get
			{
				return Res.GetString("D5F96FB1-07C9-4F4D-A319-92FDAD317AFF", "Importer is not marked as being on the CARM portal under Config > Canada > Accounting Declaration > Bond Details");
			}
		}

		internal static string ClinetIsInCARMPortalButNoBondOnFile
		{
			get
			{
				return Res.GetString("EC3675BF-788D-4D97-8DCD-E080F6AF2333", "Importer is in the CARM Portal but no bond information is on file. Please confirm that the importer has a bond");
			}
		}

		#endregion
	}
}
