using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(CINTemporaryStorageConsolController))]
	class CINTemporaryStorageConsolControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.FR.CINTemporaryStorageConsolController;
		public override Type ControllerToBashType
		{
			get { return typeof(CINTemporaryStorageConsolController); }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SetRelatedBusinessObject(consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
			Factory.Save();
			return header;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}

		protected BusinessObject GetBusinessObjectHeader()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			Factory.Save();
			return header;
		}
	}
}
