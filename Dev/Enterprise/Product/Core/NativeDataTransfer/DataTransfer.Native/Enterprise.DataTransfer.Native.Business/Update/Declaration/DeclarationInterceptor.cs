using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DataTransfer.Native.Business.XmlConstants;

namespace Enterprise.DataTransfer.Native.Business.Update.Declaration
{
	internal class DeclarationInterceptor : BaseInterceptor
	{
		internal DeclarationInterceptor(DeclarationInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}

		readonly BusinessObjectFactory factory;

		DeclarationInterceptorSetting Setting => (DeclarationInterceptorSetting)InterceptorSetting;

		public override void Invoke(IEntitySet entitySet)
		{
			var declarationCompanyEntity = entitySet.Root.Parents.FirstOrDefault(e => e.EntityName == GlbCompanySchema.Constants.TableName);
			var declarationCompanyPk = declarationCompanyEntity?.InternalPK ?? Setting.Source?.TargetCompanyPK;

			var company = factory.Load<GlbCompany>(new ZGuid(declarationCompanyPk));
			var countryCode = company?.GC_RN_NKCountryCode ?? Env.CurrentCompany.Country.Code;

			entitySet.Root.DepthFirstTraversal((entity, relative) => SetDataModel(entity, countryCode));

			Function(entitySet);
		}

		void SetDataModel(IEntity entity, string countryCode)
		{
			if (tablesWithDataModelColumn.Contains(entity.TableName) && entity.Definition.PropertyDefinitions.HasDefinition(PropertyNames.DataModel))
			{
				entity[PropertyNames.DataModel] = countryCode;
			}
		}

		readonly IImmutableSet<string> tablesWithDataModelColumn = ImmutableHashSet.Create(
			JobDeclarationSchema.Constants.TableName,
			JobComInvoiceHeaderSchema.Constants.TableName,
			JobComInvoiceLineSchema.Constants.TableName,
			CusEntryHeaderSchema.Constants.TableName,
			CusEntryLineSchema.Constants.TableName);
	}
}
