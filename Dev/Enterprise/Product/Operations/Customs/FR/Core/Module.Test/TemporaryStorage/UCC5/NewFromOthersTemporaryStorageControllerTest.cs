using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(NewFromOthersTemporaryStorageController))]
	class NewFromOthersTemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected BusinessObject GetBusinessObjectHeader()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			Factory.Save();
			return header;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(NewFromOthersTemporaryStorageController); }
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.France;
	}
}
