using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class CDSResponseStatusTestDataHelper : UniversalReferenceTestDataHelper
	{
		public CDSResponseStatusTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void CreateCDSCustomsStatuses()
		{
			CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DeclarationAccepted);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.MessageRegistered);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.MessageRejected);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.GoodsMayBeReleased);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DeclarationCleared);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DeclarationCancelled);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue);
			CreateCustomsStatusCusCodeEntry(CDS.Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion);
		}

		RefCusCodeList CreateCustomsStatusCusCodeEntry(ZString code, string description = null)
		{
			var cusCode = CreateGBCusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, code, description);

			switch (code)
			{
				case CDS.Constants.NumbericFunctionCodes.DeclarationAccepted:
					{
						cusCode.ZZD_Description = "Declaration has been legally accepted";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty),
							new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty),
							new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty),
							new KeyValuePair<string, string>("IExecuteAutoBilling", ZString.Empty),
						});
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.MessageRegistered:
					{
						cusCode.ZZD_Description = "Message has been registered";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty),
							new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty),
						});
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.MessageRejected:
					{
						cusCode.ZZD_Description = "Message rejected";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl:
					{
						cusCode.ZZD_Description = "Declaration is subject to physical control";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.DeclarationSubjectToPhysicalControl2:
					{
						cusCode.ZZD_Description = "Declaration is subject to physical control";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.GoodsMayBeReleased:
					{
						cusCode.ZZD_Description = "Goods may now be released";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty));
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.DeclarationCleared:
					{
						cusCode.ZZD_Description = "Declaration is now cleared";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty));
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.DeclarationCancelled:
					{
						cusCode.ZZD_Description = "Declaration has been canceled";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty));
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.DutiesTaxesCalculatedAndDue:
					{
						cusCode.ZZD_Description = "Duties and taxes have been calculated and are due";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty),
							new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty),
						});
						break;
					}
				case CDS.Constants.NumbericFunctionCodes.GoodsExitedCustomsUnion:
					{
						cusCode.ZZD_Description = "Goods have exited the Customs Union";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty),
						});
						break;
					}
			}

			return cusCode;
		}

		public RefCusCodeList CreateGBCusCodeListEntry(string codeType, ZString code, string description = null)
		{
			if (string.IsNullOrEmpty(description))
			{
				description = code;
			}

			return CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		void AddAttributesAndNames(RefCusCodeList cusCodeList, params KeyValuePair<string, string>[] namesAndValues)
		{
			foreach (var nameValue in namesAndValues)
			{
				var name = nameValue.Key;
				var value = nameValue.Value;
				var key = string.Join("|", name.ToUpper(), cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);

				if (!ExistingNames.Contains(key))
				{
					ExistingNames.Add(key);
					CreateNewOrGetExistingRefCusCodeListAttributeName(name, value, cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
				}
				cusCodeList.Attributes.AddNew(name, value);
			}
		}

		HashSet<string> ExistingNames => existingNames ?? (existingNames = new HashSet<string>());
		HashSet<string> existingNames;
	}
}
