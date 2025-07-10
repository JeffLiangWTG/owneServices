using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class HelpDataStringLookups : ZLookups
	{
		public HelpDataStringLookups(AutoHelpDataString parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		public CodeDescriptionPairList EditReasons
		{
			get { return new EditReasons(); }
		}
	}
}
