using System;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.G3.Common;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3RevokeHouseConsignmentWrapperTest : G3HouseConsignmentWrapperTest
	{
		public override void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET
				AssertExceptionThrown("Throws Exception if bill is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'bill')", () => new G3RevokeHouseConsignmentWrapper(null, additionalInfoWrapper));

				AssertExceptionThrown("Throws Exception if additionalInfoWrapper is null", typeof(ArgumentNullException),
					"Value cannot be null. (Parameter 'additionalInfoWrapper')", () => new G3RevokeHouseConsignmentWrapper(bill, null));
#else
				AssertExceptionThrown("Throws Exception if bill is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: bill", () => new G3RevokeHouseConsignmentWrapper(null, additionalInfoWrapper));

				AssertExceptionThrown("Throws Exception if additionalInfoWrapper is null", typeof(ArgumentNullException),
					"Value cannot be null.\r\nParameter name: additionalInfoWrapper", () => new G3RevokeHouseConsignmentWrapper(bill, null));
#endif
			});
		}

		public void TestAdditionalInformation()
		{
			var revokeWrapper = wrapper as G3RevokeHouseConsignmentWrapper;

			CombineAssertions(() =>
			{
				var additionalInfo = revokeWrapper.AdditionalInformation;
				AssertNotNull("Expected filled AdditionalInformation", additionalInfo);
				AssertEquals("Expected filled AdditionalInformation Number", "G3001", additionalInfo.Number);
				AssertEquals("Expected filled AdditionalInformation Name", "invalid reason", additionalInfo.Name);

				AssertSame("Cached AdditionalInformation", additionalInfo, revokeWrapper.AdditionalInformation);
			});
		}

		protected override G3HouseConsignmentWrapper CreateWrapper()
		{
			additionalInfoWrapper = new DocumentCommonWrapper("invalid reason", "G3001");
			return new G3RevokeHouseConsignmentWrapper(bill, additionalInfoWrapper);
		}

		DocumentCommonWrapper additionalInfoWrapper;
	}
}
