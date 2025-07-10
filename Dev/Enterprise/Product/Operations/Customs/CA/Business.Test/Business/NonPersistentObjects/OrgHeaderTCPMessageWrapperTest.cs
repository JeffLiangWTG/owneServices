using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OrgHeaderTCPMessageWrapper))]
	sealed class OrgHeaderTCPMessageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCollection()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			message.EM_LinkedObject = Organisation;

			OrgHeaderTCPMessageWrapper wrapper = GetNewWrapper();
			AssertNotNull(wrapper.Messages);
			AssertEquals(1, wrapper.Messages.Count);
			AssertEquals(true, wrapper.Messages.Contains(message.PK));
		}

		OrgHeaderTCPMessageWrapper GetNewWrapper()
		{
			return (OrgHeaderTCPMessageWrapper)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgHeaderTCPMessageWrapper.New(Organisation);
		}

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.FillWithValidTestData();
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;
	}
}
