using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ConfigurationUserControl : ZUserControl
	{
		public ConfigurationUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var system = dataSource as BMSystem;
			if (system != null)
			{
				var factory = system.Factory;

				foreach (var releaseGroup in system.ReleaseGroups)
				{
					factory.AddFetchHint(GlbGroupSchema.PK, releaseGroup.FSG_GG_Group);
				}
			}
		}
	}
}
