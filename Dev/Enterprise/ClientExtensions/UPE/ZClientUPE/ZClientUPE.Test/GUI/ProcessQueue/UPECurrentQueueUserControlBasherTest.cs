using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(FormForCurrentQueueUserControl))]
	internal class UPECurrentQueueUserControlBasherTest : ZArchitecture.GUI.Testing.BasherTest
	{
		public override Form GetFormToBash()
		{
			return new FormForCurrentQueueUserControl(BusinessEntity);
		}

		protected BusinessObject BusinessEntity
		{
			get
			{
				ProcessQueue processQueue = Factory.New<ProcessQueue>();
				return ActiveProcessQueue.New(processQueue);
			}
		}

		protected virtual string BindToPrefix
		{
			get
			{
				return "";
			}
		}
	}
}
