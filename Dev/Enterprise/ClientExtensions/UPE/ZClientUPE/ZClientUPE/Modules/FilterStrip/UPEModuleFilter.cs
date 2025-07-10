using System;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Module
{
	public class UPEModuleFilter : ModuleFilter
	{
		public UPEModuleFilter(QueueFilterHelper queueFilterHelper)
			: base("Queue Status")
		{
			QueueFilterHelper = queueFilterHelper;
		}

		readonly QueueFilterHelper QueueFilterHelper;

		#region GetNewCommonModuleFilter / CopyPersistantValuesFromFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			throw new NotSupportedException(GetType().Name + " does not support copying.");
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			QueueFilterHelper.FilterBizObj.QueueStatus.Clear();
		}

		protected override bool IsEmptyCore => QueueFilterHelper.FilterBizObj.QueueStatus.IsEmpty;

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return UPEFilterConstants.UPEFilterCategory; }
		}

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new UPEModuleFilterValidation(this);
		}

		#endregion

		protected override bool ShouldReevaluateQuery()
		{
			return true;
		}

		#region Query

		protected override ZQuery GetQuery()
		{
			ZQuery result = new ZQuery();
			QueueFilterHelper.AddQueueNameAndStatusesToFilter(result);

			return result;
		}

		protected override object[] QueryDelegateParameters
		{
			get { throw new NotSupportedException(GetType().Name + " does not support filtering via a query delegate."); }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException(GetType().Name + " does not support filtering via filter columns.");
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			// currently not serialised
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			// currently not serialised
		}

		#endregion

		#region Test Data Setup

#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
		}

#endif
		#endregion
	}

	#region class Validation

	public class UPEModuleFilterValidation : ModuleFilterValidation
	{
		public UPEModuleFilterValidation(UPEModuleFilter parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
	}

	#endregion
}
