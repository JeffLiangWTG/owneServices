using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	public class EdiUserAgreementTypesMapperTest : TestCaseWithFactory
	{
		public void TestMapper()
		{
			var list = new EdiUserAgreementTypes();
			var missedKeys = new List<string>();
			var inValidDescriptions = new List<string>();
			foreach (CodeDescriptionPair item in list)
			{
				if (!Mapper.TryGetValue(item.Code, out var info))
				{
					missedKeys.Add(item.Code);
				}
				else if(list.GetDescriptionFromCode(item.Code) != info.Description)
				{
					inValidDescriptions.Add($"[Code]{item.Code}; [Mapper Description]{info.Description}; [Expected]{item.Description}");
				}
			}

			CombineAssertions("", () =>
			{
				AssertEquals($"The following keys \r\n{string.Join(System.Environment.NewLine, missedKeys)}\r\n are not added into EdiUserAgreementTypesMapper.Mapper", 0, missedKeys.Count);
				AssertEquals($"The description of the following keys \r\n{string.Join(System.Environment.NewLine, inValidDescriptions)}\r\n in Mapper is invalid.", 0, missedKeys.Count);
			});
		}

		public void TestGetAgreementLevel()
		{
			var expectedType = new Dictionary<string, string>
			{
				{ EdiUserAgreementTypes.Codes.CargoWiseNext, EdiUserAgreementLevelList.Codes.Corporate },
			};

			foreach (var item in Mapper)
			{
				if (expectedType.TryGetValue(item.Key, out var agreementType))
				{
					AssertEquals(agreementType, EdiUserAgreementTypesMapper.GetAgreementLevel(item.Key));
				}
				else
				{
					AssertEquals(EdiUserAgreementLevelList.Codes.User, EdiUserAgreementTypesMapper.GetAgreementLevel(item.Key));
				}
			}
		}

		public void TestIsAgreementTypeWithVariant()
		{
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(null));
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(string.Empty));

			Assert(Mapper[EdiUserAgreementTypes.Codes.CargoWiseNext].EnableVariant);
			Assert(EdiUserAgreementTypesMapper.IsVariantEnabled(EdiUserAgreementTypes.Codes.CargoWiseNext));

			Assert(!Mapper[EdiUserAgreementTypes.Codes.CreditCheckService].EnableVariant);
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(EdiUserAgreementTypes.Codes.CreditCheckService));
		}

		public void TestGetParentAvailableAgreementTypeList()
		{
			var result = EdiUserAgreementTypesMapper.GetParentAvailableAgreementTypeList(LicenceEnterpriseSchema.Constants.Prefix);
			AssertEquals(1, result.Count);
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, result[0].Code);

			var codeNullResult = EdiUserAgreementTypesMapper.GetParentAvailableAgreementTypeList(null);
			AssertEquals(0, codeNullResult.Count);
		}

		public void TestGetBindingTableCodes()
		{
			var tableCodes = EdiUserAgreementTypesMapper.GetBindingTableCodes(EdiUserAgreementTypes.Codes.CargoWiseNext);
			AssertEquals("LicenceEnterprise is a valid parent for CargoWise Next agreements", true, tableCodes.Contains(LicenceEnterpriseSchema.Constants.Prefix));
			AssertEquals("LicenceDatabase is a valid parent for CargoWise Next agreements", true, tableCodes.Contains(LicenceDatabaseSchema.Constants.Prefix));
			AssertEquals("Should not include random codes", false, tableCodes.Contains("ZZZ"));

			tableCodes = EdiUserAgreementTypesMapper.GetBindingTableCodes(EdiUserAgreementTypes.Codes.CreditCheckService);
			AssertEquals("Other agreement types should not have binding", 0, tableCodes.Count);
		}

		static ReadOnlyDictionary<string, AgreementTypeInfo> Mapper => (ReadOnlyDictionary<string, AgreementTypeInfo>)typeof(EdiUserAgreementTypesMapper).GetField("Mapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
	}
}
