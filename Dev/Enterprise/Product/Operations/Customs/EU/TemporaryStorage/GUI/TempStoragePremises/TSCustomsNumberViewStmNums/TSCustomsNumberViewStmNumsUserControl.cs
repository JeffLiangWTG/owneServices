using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TSCustomsNumberViewStmNumsUserControl : MasterFiles.GUI.CustomsNumberViewStmNumsUserControl
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public TSCustomsNumberViewStmNumsUserControl()
		{
			InitializeComponent();
		}

		public TSCustomsNumberViewStmNumsUserControl(TSCustomsNumberViewStmNumsWrapperCollection collection)
		: base(collection)
		{
			InitializeComponent();
		}

		protected override MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider GetProviderForStandAloneFactory(BusinessObjectFactory factory, string providerKey, ZGuid parentPK)
			=> new Business.TSCustomsNumberViewStmNumsBusinessProviderFactory(factory).GetProvider(providerKey, parentPK);
	}
}
