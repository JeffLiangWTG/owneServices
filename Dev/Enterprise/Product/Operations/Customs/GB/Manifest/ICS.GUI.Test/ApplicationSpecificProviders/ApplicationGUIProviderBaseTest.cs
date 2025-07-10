using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	public abstract class ApplicationGUIProviderBaseTest<T, THeader> : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<T, THeader>
		where T : ApplicationGUIProviderBase
		where THeader : AsycudaManifestHeaderBase
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);
		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(ASYCUDA.GUI.AsycudaPackUserControl) };
		protected override Type ExpectedBillLayoutType => typeof(BillLayouts);
		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[] { typeof(ItineraryForManifestHeaderUserControl) };
		protected abstract ZString ManifestType { get; }

		protected override THeader CreateNewManifest()
		{
			var result = base.CreateNewManifest();
			result.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			result.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			result.AMA_ManifestType = ManifestType;
			return result;
		}
	}
}
