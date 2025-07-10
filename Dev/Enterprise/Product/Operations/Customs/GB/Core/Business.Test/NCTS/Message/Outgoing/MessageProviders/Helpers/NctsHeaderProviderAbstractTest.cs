using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestsSubclassesOf(typeof(NctsHeaderSharedDataProvider))]
	public abstract class NctsHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : NctsHeaderSharedDataProvider
	{
		public void TestMessageType()
		{
			AssertEquals(MessageType, Provider.MessageType);
		}

		public void TestCorrelationIdentifier()
		{
			AssertNotNull(Provider.CorrelationIdentifier);
			AssertType<string>(Provider.CorrelationIdentifier);
			AssertEquals(Provider.MessageIdentification, Provider.CorrelationIdentifier);
		}

		protected abstract string MessageType { get; }

		protected abstract string MovementType { get; }

		protected override T GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			provider = (T)Activator.CreateInstance(typeof(T), nctsHeader);
		}

		protected T provider;
		protected NctsHeader nctsHeader;

		protected void CreatePrincipal()
		{
			var principal = Factory.New<OrgHeader>();
			principal.OH_FullName = "HolderName";
			principal.OH_Code = "HOLDER";
			var principalAddress = principal.Addresses.AddNew();
			principalAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			principalAddress.OA_Address1 = "Sherwood Drive";
			principalAddress.OA_City = "Milton Keynes";

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "HolderID", Core.Constants.CountryCodes.UnitedKingdom),
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.UnitedKingdom)
			};
			principal.CustomsCodes.AddRange(orgCusCodes);

			Factory.CreateJobDocAddress(AutoDocAddressTypes.Codes.Principal, contactName: "ContactName", contactEmail: "ContactEmail", contactPhone: "ContactPhone", orgCusCodes: orgCusCodes, orgHeader: principal, parent: nctsHeader, jobDocAddress: nctsHeader.Principal);
		}
	}
}
