using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsHeaderSharedDataProvider))]
	public abstract class NctsHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : NctsHeaderSharedDataProvider
	{
		public void TestMessageType()
		{
			AssertEquals(MessageType, Provider.MessageType);
		}

		protected abstract string MessageType { get; }

		protected abstract string MovementType { get; }

		protected virtual bool HasSendingActionParameter => false;

		protected override T GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			CreateProvider();
		}

		protected virtual void CreateProvider()
		{
			if (HasSendingActionParameter)
			{
				switch (MovementType)
				{
					case NctsMovementType.Codes.Arrival:
						action = new MessageSendingAction(nctsHeader.ArrivalMovementHeader);
						break;
					default:
						action = new MessageSendingAction(nctsHeader.MovementHeader);
						break;
				}

				provider = (T)Activator.CreateInstance(typeof(T), action);
			}
			else
			{
				provider = (T)Activator.CreateInstance(typeof(T), nctsHeader);
			}
		}

		protected T provider;
		protected NctsHeader nctsHeader;
		protected MessageSendingAction action;

		protected void CreatePrincipal()
		{
			var principal = Factory.New<OrgHeader>();
			principal.OH_FullName = "HolderName";
			principal.OH_Code = "HOLDER";
			var principalAddress = principal.Addresses.AddNew();
			principalAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			principalAddress.OA_Address1 = "Wapenstilstandlaan 47";
			principalAddress.OA_City = "Antwerpen";

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "HolderID", Core.Constants.CountryCodes.Belgium),
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			principal.CustomsCodes.AddRange(orgCusCodes);

			Factory.CreateJobDocAddress(AutoDocAddressTypes.Codes.Principal, contactName: "ContactName", contactEmail: "ContactEmail", contactPhone: "ContactPhone", orgCusCodes: orgCusCodes, orgHeader: principal, parent: nctsHeader, jobDocAddress: nctsHeader.Principal);
		}
	}
}
