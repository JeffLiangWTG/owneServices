using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageController))]
	class TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactoryWithApplicationCode()
		{
			var controller = new TemporaryStorageController(FRConstants.TemporaryStorage.AppCodeFRC);
			using (var form = controller.ShowNewForm() as ZForm)
			{
				var jobHeader = (CusTempStorageJobHeader)form.BusinessEntity;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeFRC, jobHeader.SJH_AppCode);
				AssertNotNull(jobHeader.CusTempStorageDec);
				AssertEquals(FRConstants.TemporaryStorage.AppCodeFRC, jobHeader.CusTempStorageDec.STH_DeclarationType);
				AssertEquals(0, jobHeader.CusTempStorageDec.CusTempStorageLines.Count);
			}

			controller = new TemporaryStorageController(FRConstants.TemporaryStorage.AppCodeIST);
			using (var form = controller.ShowNewForm() as ZForm)
			{
				var jobHeader = (CusTempStorageJobHeader)form.BusinessEntity;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, jobHeader.SJH_AppCode);
				AssertNotNull(jobHeader.CusTempStorageDec);
				AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, jobHeader.CusTempStorageDec.STH_DeclarationType);
				AssertEquals(1, jobHeader.CusTempStorageDec.CusTempStorageLines.Count);
			}

			controller = new TemporaryStorageController(FRConstants.TemporaryStorage.AppCodeLAD);
			using (var form = controller.ShowNewForm() as ZForm)
			{
				var jobHeader = (CusTempStorageJobHeader)form.BusinessEntity;
				AssertEquals(FRConstants.TemporaryStorage.AppCodeLAD, jobHeader.SJH_AppCode);
				AssertNotNull(jobHeader.CusTempStorageDec);
				AssertEquals(FRConstants.TemporaryStorage.AppCodeLAD, jobHeader.CusTempStorageDec.STH_DeclarationType);
				AssertEquals(1, jobHeader.CusTempStorageDec.CusTempStorageLines.Count);
			}
		}

		protected BusinessObject GetBusinessObjectHeader()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			Factory.Save();
			return header;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(TemporaryStorageController); }
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.France;
	}
}
