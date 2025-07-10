using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.Container
{
	public class ContainerInterceptor : BaseInterceptor
	{
		readonly BusinessObjectFactory factory;

		EntityRepository entityRepository;

		public ContainerInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}

		public override void Invoke(IEntitySet entitySet)
		{
			var entityAction = entitySet?.Root?.Action;

			if (entityAction == EntityAction.INSERT || entityAction == EntityAction.UPDATE || entityAction == EntityAction.MERGE)
			{
				CheckEntityValid(entitySet.Root, entityAction.Value);
			}

			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void CheckEntityValid(IEntity container, EntityAction entityAction)
		{
			try
			{
				entityRepository = new EntityRepository(InterceptorSetting.Context, sessionServices);
				entityRepository.OpenSession();

				var containerCode = container.GetPropertyOrBlankString("Code");
				var matchedEntity = entityRepository.FindRow(container, false);

				switch (entityAction)
				{
					case EntityAction.INSERT:
						{
							if (IsInvalidForInsert(containerCode))
							{
								Fail(containerCode);
							}

							break;
						}
					case EntityAction.UPDATE:
						{
							if (IsInvalidForUpdate(containerCode, matchedEntity))
							{
								Fail(containerCode);
							}

							break;
						}
					case EntityAction.MERGE:
						{
							if (IsInvalidForUpdate(containerCode, matchedEntity))
							{
								Fail(containerCode);
							}

							if (matchedEntity == null && IsInvalidForInsert(containerCode))
							{
								Fail(containerCode);
							}

							break;
						}
				}
			}
			finally
			{
				entityRepository = null;
			}
		}

		void Fail(string containerCode) => throw new NativeXMLUserVisibleException($"The specified container code {containerCode} already exists.");

		bool IsInvalidForInsert(string containerCode) => GetMatchedContainerWithSameCode(containerCode, Guid.Empty) != null;

		bool IsInvalidForUpdate(string containerCode, DataRow matchedEntity)
		{
			if (matchedEntity == null)
			{
				return false;
			}

			var matchedEntityPK = new Guid(matchedEntity.ItemArray[matchedEntity.Table.Columns.IndexOf(RefContainerSchema.Constants.PK)].ToString());
			var matchedCode = matchedEntity.ItemArray[matchedEntity.Table.Columns.IndexOf(RefContainerSchema.Constants.RC_Code)].ToString();

			return matchedEntity != null && matchedCode != containerCode && GetMatchedContainerWithSameCode(containerCode, matchedEntityPK) != null;
		}

		RefContainer GetMatchedContainerWithSameCode(string code, Guid excludedPK)
		{
			var query = new ZDBOnlyQuery(typeof(RefContainer));
			query.AddToFilter(RefContainerSchema.RC_Code, code);
			query.AddToFilter(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, excludedPK);

			return factory.LoadTop1<RefContainer>(query);
		}
	}
}
