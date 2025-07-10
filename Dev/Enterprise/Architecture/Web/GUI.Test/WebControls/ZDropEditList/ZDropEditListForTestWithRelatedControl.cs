using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDropEditListForTestWithRelatedControl : ZDropEditListForTest
	{
		protected override ZString RelatedControlID
		{
			get
			{
				return "some_related_control_identifier";
			}
		}
	}
}
