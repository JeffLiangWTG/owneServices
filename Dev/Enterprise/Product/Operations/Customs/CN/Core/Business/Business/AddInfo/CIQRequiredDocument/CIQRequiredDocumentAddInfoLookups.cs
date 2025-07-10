//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCIQRequiredDocumentAddInfoLookups
//
//    This class should be used for overriding collections in AutoCIQRequiredDocumentAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CIQRequiredDocumentAddInfoLookups : AutoCIQRequiredDocumentAddInfoLookups
	{
		public CIQRequiredDocumentAddInfoLookups(AutoCIQRequiredDocumentAddInfo parent) : base(parent)
		{
		}

		public new AutoCIQRequiredDocumentAddInfo Parent => (AutoCIQRequiredDocumentAddInfo)base.Parent;

		public CIQRequiredDocument CIQRequiredDocument => Parent.Parent as CIQRequiredDocument;

		public ICodeDescriptionPairList CIQRequiredDocumentTypes
		{
			get
			{
				CodeDescriptionPairList result;
				var declaration = CIQRequiredDocument?.CusEntryInstruction?.JobDeclaration;
				if (declaration != null)
				{
					if (declaration.IsImport)
					{
						result = Factory.GetCachedValue("CNCIQRequiredDocumentTypes_IMP", () =>
						{
							var list = new CIQRequiredDocumentTypeList();
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._20);
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._22);
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._96);
							return list;
						});
					}
					else
					{
						result = Factory.GetCachedValue("CNCIQRequiredDocumentTypes_EXP", () =>
						{
							var list = new CIQRequiredDocumentTypeList();
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._21);
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._24);
							list.RemoveCode(CIQRequiredDocumentTypeList.Codes._95);
							return list;
						});
					}
				}
				else
				{
					result = new CodeDescriptionPairList();
				}
				return result;
			}
		}
	}
}
