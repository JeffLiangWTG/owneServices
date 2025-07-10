using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	public abstract class ASYCUDAManifestBillModuleAbstractTest : ZModuleBasherTest
	{
		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			return result;
		}

		protected void CheckAndCloseForm<T>(ControllerID controllerId, ZGuid linkedEntityPk, ZGuid formEntityPk, ODisplayMode displayMode)
		{
			using (var form = (IZForm)OpenedFormCache.GetInstance().GetForm(linkedEntityPk.ToGuid(), controllerId.ToString()))
			{
				AssertNotNull($"{controllerId}:{linkedEntityPk} should be in cache", form);
				AssertEquals($"{controllerId}:{linkedEntityPk} should have type {typeof(T).Name} but was {form.GetType().Name}", true, form is T);
				Application.DoEvents();
				AssertEquals($"{controllerId}:{linkedEntityPk}", formEntityPk, form.BusinessEntityForPersistingForm.Identifier);
				AssertEquals($"{controllerId}:{linkedEntityPk}", displayMode, form.DisplayMode);
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			var factory = new BusinessObjectFactory();
			var headerSG = (AsycudaManifestHeader)factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			headerSG.FillWithValidTestData();
			CreateData(headerSG);
			CreateData((AsycudaManifestHeader)factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>());
			var header = (AsycudaManifestHeader)factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			CreateData(header);
			CreateData((AsycudaManifestHeader)factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>());
			factory.Save();
		}

		protected void CreateData(AsycudaManifestHeader header)
		{
			header.AMA_Nature = "A";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = header.AMA_RN_NKCountry + "HB1";
			bill.ABL_ShipmentType = "EXP";
		}
	}
}
