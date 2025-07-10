using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.ARAP
{
	public class LinkedeNettEDIMessagePlugin : ZPlugIn
	{
		public LinkedeNettEDIMessagePlugin(IBusiness hostEntity)
			: base(hostEntity)
		{
			BusinesEntity = (TransactionHeader)hostEntity;
		}

		readonly TransactionHeader BusinesEntity;

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new LinkedeNettEDIMessagePluginUserControl();
		}

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return (NoResString)"EDI Messages"; } // Hard-coded constant
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		public override void Delete()
		{
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return BusinesEntity.EDIMessages;
		}
	}
}
