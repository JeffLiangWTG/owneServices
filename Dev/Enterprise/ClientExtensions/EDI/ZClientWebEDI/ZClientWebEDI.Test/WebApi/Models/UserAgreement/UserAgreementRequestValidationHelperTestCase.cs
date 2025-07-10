using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class UserAgreementRequestValidationHelperTestCase : TestCaseWithFactory
	{
		public void TestIsValidType()
		{
			AssertEquals(false, UserAgreementRequestValidationHelper.IsValidAgreementType("AAA"));
			foreach (var type in new EdiUserAgreementTypes().GetAllCodes())
			{
				AssertEquals(true, UserAgreementRequestValidationHelper.IsValidAgreementType(type));
			}
		}

		public void TestCanBypassUserAccountCollectionAgreement()
		{
			var product = "ABC";

			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var newList = new CodeDescriptionPairList();
				newList.AddPair(product, "Alphabet");
				return newList;
			});
			var productBypassList = new CodeSelectionCollection(listProvider);
			var productCodeSelection = productBypassList.AddNew();
			productCodeSelection.Code = product;
			EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productBypassList);

			AssertEquals(true, UserAgreementRequestValidationHelper.CanBypassUserAccountCollectionAgreement(product));
			AssertEquals(true, UserAgreementRequestValidationHelper.CanBypassUserAccountCollectionAgreement(product.ToLower()));
			AssertEquals(false, UserAgreementRequestValidationHelper.CanBypassUserAccountCollectionAgreement("ZZZ"));
		}
	}
}
