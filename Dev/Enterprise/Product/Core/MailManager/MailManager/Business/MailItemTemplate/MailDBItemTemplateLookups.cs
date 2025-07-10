//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBItemTemplateLookups
//
//    This class should be used for overriding collections in AutoMailDBItemTemplateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.MailManager.Business
{
	public class MailDBItemTemplateLookups : AutoMailDBItemTemplateLookups
	{
		protected MailDBItemTemplateLookups() : this(null) { }
		internal MailDBItemTemplateLookups(AutoMailDBItemTemplate parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList MailTemplateCategories
		{
			get { return GetNewMailTemplateCategories(); }
		}

		CodeDescriptionPairList GetNewMailTemplateCategories()
		{
			var list = GetNewMailTemplateCategoriesCore();
			list.SortByDescription();
			return list;
		}

		protected virtual CodeDescriptionPairList GetNewMailTemplateCategoriesCore()
		{
			return new MailTemplateCategoryList();
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		public static MailDBItemTemplateLookups New()
		{
			MailDBItemTemplateLookups result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new MailDBItemTemplateLookups();
			}
			return result;
		}

		protected delegate MailDBItemTemplateLookups NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = (Parent == null || base.Factory == null) ? new BusinessObjectFactory() : base.Factory;
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;
	}
}
