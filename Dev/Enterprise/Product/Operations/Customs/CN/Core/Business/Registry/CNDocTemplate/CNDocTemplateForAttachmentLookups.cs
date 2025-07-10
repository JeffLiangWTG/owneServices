using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CNDocTemplateForAttachmentLookups : ZLookups
	{
		public CNDocTemplateForAttachmentLookups(CNDocTemplateForAttachment parent)
			 : base(parent)
		{
		}

		protected new CNDocTemplateForAttachment Parent => (CNDocTemplateForAttachment)base.Parent;

		protected new BusinessObjectFactory Factory => Parent.Factory;

		public OrgHeaderCollection Organizations => new OrgHeaderCollection(Factory);

		public UntranslatableCodeDescriptionPairList DocumentTemplateList
		{
			get
			{
				return Factory.GetCachedValue(System.FormattableString.Invariant($"CN_DocumentTemplateList_{Parent.DataContext}"), () =>
				{
					var result = new UntranslatableCodeDescriptionPairList((NoResString)"DocumentTemplateList is not translatable");
					var filter = new ZQuery();
					filter.AddToFilter(StmTemplateSchema.SO_DataContext, Parent.DataContext);
					var templates = Factory.Load<StmTemplate>(filter);

					foreach (var template in templates)
					{
						var templateName = template.SO_Name;
						if (template.SO_IsSystemDefined)
						{
							templateName += (NoResString)"(System)";
						}
						result.AddPair(templateName, templateName);
					}

					return result;
				});
			}
		}

		public RefDocTypeCollection DocumentTypeList => new RefDocTypeCollection(Factory);

		public UntranslatableCodeDescriptionPairList AttachmentTypeList => Factory.GetCachedValue<CSDDocTypeList>();
	}
}
