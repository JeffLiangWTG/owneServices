using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management
{
	public class UniversalJobLinkCreator
	{
		public UniversalJobLinkCreator(BusinessObjectFactory businessFactory, BusinessObject parentBO,
			IOrgHeader sourceOrg,
			IDataContextDataObject sourceDataContext, IXmlImportLogger logger, bool sourceOverride = false)
		{
			BusinessFactory = Argument.NotNull(businessFactory, "businessFactory");
			ParentBO = Argument.NotNull(parentBO, "parentBO");
			SourceOrg = sourceOrg;
			SourceDataContext = Argument.NotNull(sourceDataContext, "sourceDataContext");
			Logger = Argument.NotNull(logger, "logger");
			SourceOverride = sourceOverride;
		}

		readonly BusinessObjectFactory BusinessFactory;
		readonly BusinessObject ParentBO;
		readonly IOrgHeader SourceOrg;
		readonly IDataContextDataObject SourceDataContext;
		readonly IXmlImportLogger Logger;
		readonly bool SourceOverride;

		internal RowFactory RowFactory
		{
			get { return BusinessFactory is IUniversalBusinessObjectFactory universalBOFactory ? universalBOFactory.RowFactory : ((IBusinessObjectFactoryInternals)BusinessFactory).RowFactory; }
		}

		#region DataProvider Codes

		string EnterpriseCode => SourceOverride ? Registration.Key.EnterpriseCode : Logger.TopLevelDataContext.GetEnterpriseCode();
		string ServerCode => SourceOverride ? Registration.Key.ServerCode : Logger.TopLevelDataContext.GetServerCode();
		string CompanyCode => SourceOverride ? Env.CurrentCompany.Code : Logger.TopLevelDataContext.GetCompanyCode();

		IProductRegistration Registration => registration ??= ObjectFactory.New<IProductRegistration>();
		IProductRegistration registration;

		#endregion

		#region IsInternal

		bool IsInternal => Logger.IsInternalImport() || SourceOrg == null && SourceOverride;

		#endregion

		#region DataSources

		IEnumerable<IDataSourceDataObject> DataSources
			=> (dataSources ?? (dataSources = SourceDataContext.DataSourceCollection.Where(s => s.Type.HasValue).ToArray()));
		IEnumerable<IDataSourceDataObject> dataSources;

		#endregion

		#region ExistingLinks

		IEnumerable<IColumnIndexer> ExistingLinks => existingLinks ?? (existingLinks = GetExistingLinks());
		IEnumerable<IColumnIndexer> existingLinks;

		IEnumerable<IColumnIndexer> GetExistingLinks()
		{
			return (ParentBO.IsInDatabase
						? RowFactory.Load(StmUniversalJobLinkSchema.Constants.TableName,
										new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, ParentBO.PK))
											.Select(DataObjectReader.GetColumnIndexerFromRow).ToArray()
						: Enumerable.Empty<IColumnIndexer>());
		}

		#endregion

		#region CreateJobLink

		public void TryCreateJobLink(DataContextType dataContextType)
		{
			if (SourceDataContext != null
				&& SourceDataContext.DataSourceCollection != null
				&& IsValidCode(EnterpriseCode)
				&& IsValidCode(ServerCode)
				&& IsValidCode(CompanyCode))
			{
				foreach (var source in DataSources.Where(d => d.Type.HasValue && d.Type.Value == dataContextType.ToString()
																	&& d.Key.HasValue && !d.Key.Value.IsEmpty))
				{
					CreateJobLink(source);
				}
			}
		}

		bool IsValidCode(string code) => code.Length == 3;

		void CreateJobLink(IDataSourceDataObject source)
		{
			var linkRow = ExistingLinks.SingleOrDefault(FindMatchingLinks(source)) ??
								RowFactory.NewRowWithPK(StmUniversalJobLinkSchema.Instance);

			SetupJobLink(source, linkRow);
		}

		void SetupJobLink(IDataSourceDataObject source, IColumnIndexer linkRow)
		{
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_EnterpriseCode, EnterpriseCode, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_ServerCode, ServerCode, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_CompanyCode, CompanyCode, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_ParentID, ParentBO.PK, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_ParentTableCode, ParentBO.TablePrefix, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SourceType, source.Type, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SourceKey, source.Key, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_OH_Owner, IsInternal ? ZGuid.Empty : SourceOrg.PK, Logger);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SystemCreateTimeUtc, ZDateTime.UtcNow);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SystemCreateUser, new ZString(User.InterchangeUserCode));
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SystemLastEditTimeUtc, ZDateTime.UtcNow);
			linkRow.SetValue(StmUniversalJobLinkSchema.UCL_SystemLastEditUser, new ZString(User.InterchangeUserCode));
		}

		Func<IColumnIndexer, bool> FindMatchingLinks(IDataSourceDataObject source)
		{
			return e => (e.GetValue(StmUniversalJobLinkSchema.UCL_SourceType) == source.Type.Value
								&& e.GetValue(StmUniversalJobLinkSchema.UCL_EnterpriseCode) == EnterpriseCode
								&& e.GetValue(StmUniversalJobLinkSchema.UCL_ServerCode) == ServerCode
								&& e.GetValue(StmUniversalJobLinkSchema.UCL_CompanyCode) == CompanyCode);
		}

		#endregion
	}
}
