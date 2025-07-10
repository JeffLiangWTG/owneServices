using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EHubNativeOrgXmlSender
	{
		public string SendMessageToEHubForAllUsReceivableOrganizations(Action<string, int> setStatusAndPercent)
		{
			setStatusAndPercent(Res.GetString("62D7B9BB-985D-483B-999F-CF1C8054B4D1", "Send organization(s) starts"), 0);

			var query = new ZDBOnlyQuery(typeof(EDIOrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

			var companySubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			companySubQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);
			query.AddSubQuery(companySubQuery, JoinCondition.And);

			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

			var capabilityQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			capabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, true);
			capabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);
			addressSubQuery.AddSubQuery(capabilityQuery, JoinCondition.And);

			query.AddSubQuery(addressSubQuery, JoinCondition.And);

			var organizationsNeedToSendReader = GetReader(query);
			var totalCount = organizationsNeedToSendReader.ApproximateCount;
			setStatusAndPercent(Res.GetString("B4C3F898-45D2-4432-931E-499732083611", "Found {0} organization(s)", totalCount), 0);

			var successfulSendOrgCount = 0;
			var failedSendOrgCount = 0;
			EDIOrgHeader lastOrgHeader = null;
			EDIOrgHeader[] orgHeaders;
			var failedMessages = new ZStringBuilder();
			while (totalCount > 0 && (orgHeaders = organizationsNeedToSendReader.LoadNextBatchInANewFactory(lastOrgHeader).OfType<EDIOrgHeader>().ToArray()).Length > 0)
			{
				using (organizationsNeedToSendReader.Factory.AddDisposableService())
				{
					foreach (var orgHeader in orgHeaders)
					{
						var result = SendMessageToEHub(orgHeader);

						if (result.Succeeded)
						{
							successfulSendOrgCount++;
							setStatusAndPercent(Res.GetString("231CCABF-E42B-49CC-839C-A6186F0BF6B9", "Send {0} - {1} finished", orgHeader.OH_Code, orgHeader.OH_FullName), (successfulSendOrgCount + failedSendOrgCount) * 100 / totalCount);
						}
						else
						{
							failedSendOrgCount++;
							var failedMessage = Res.GetString("4D355A2F-A96C-4FEB-9CF9-8F51A0D1B1C4", "Send {0} - {1} failed: {2} {3}", orgHeader.OH_Code, orgHeader.OH_FullName, result.FailureReason, result.Exception?.Message);
							failedMessages.Append(failedMessage);
							setStatusAndPercent(failedMessage, (successfulSendOrgCount + failedSendOrgCount) * 100 / totalCount);
						}
					}

					lastOrgHeader = orgHeaders[orgHeaders.Length - 1];
					setStatusAndPercent(Res.GetString("C55C16F1-743B-4F45-90CB-4691D6D36BDE", "Saving to database"), (successfulSendOrgCount + failedSendOrgCount) * 100 / totalCount);

					organizationsNeedToSendReader.Factory.Save(); // TODO: What if save fails with a concurrency error though?
				}
			}

			var sendResult = Res.GetString("3ACAF59C-73CB-47AF-864A-2C2F8B687D32", "Successfully sent {0} organization(s), failed to sent {1} organization(s)", successfulSendOrgCount, failedSendOrgCount);

			if (!failedMessages.IsEmpty)
			{
				sendResult = sendResult + System.Environment.NewLine + failedMessages.ToStringWithNewLineBetweenAppends();
			}

			setStatusAndPercent(Res.GetString("5342B070-A004-40EA-9FC8-2BECD8B877AC", "Send organization(s) ends"), 100);
			return sendResult;
		}

		protected virtual FilteredBusinessObjectReader<EDIOrgHeader> GetReader(ZDBOnlyQuery query)
		{
			return new FilteredBusinessObjectReader<EDIOrgHeader>(query);
		}

		protected virtual IDeliveryResult SendMessageToEHub(EDIOrgHeader orgHeader)
		{
			return EHubNativeOrgXmlSenderHelper.SendMessageToEHub(orgHeader);
		}
	}
}
