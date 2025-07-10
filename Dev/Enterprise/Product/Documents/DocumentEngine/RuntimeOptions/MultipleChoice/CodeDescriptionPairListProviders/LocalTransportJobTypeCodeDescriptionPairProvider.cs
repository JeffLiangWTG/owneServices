using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class LocalTransportJobTypeCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			if (localTransportJobTypes == null)
			{
				localTransportJobTypes = new CodeDescriptionPairList();
				localTransportJobTypes.AddRange(GetLocalTransportJobTypes());
			}
			return localTransportJobTypes;
		}
		CodeDescriptionPairList localTransportJobTypes;

		CodeDescriptionPairList GetLocalTransportJobTypes()
		{
			var filter = new ZQuery(LocalCartageJobTypeSchema.E3_IsHidden, false);
			filter.AddToFilter(JoinCondition.And, LocalCartageJobTypeSchema.E3_JobType, SQLComparisonOperator.NotEqual, "");

			var factory = new BusinessObjectFactory();
			var types = (ICommonCartageType[])factory.Load(ObjectFactory.GetType<ICommonCartageType>(), filter);

			var result = new CodeDescriptionPairList();
			foreach (ICommonCartageType type in types)
			{
				result.AddPair(type.E3_JobType, type.E3_Description);
			}
			result.SortByDescription();

			return result;
		}
	}
}
