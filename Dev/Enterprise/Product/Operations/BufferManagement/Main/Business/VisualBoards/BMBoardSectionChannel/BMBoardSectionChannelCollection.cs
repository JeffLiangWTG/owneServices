using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionChannelCollection : ActiveBusinessObjectCollection<BMBoardSectionChannel>
	{
		public BMBoardSectionChannelCollection(BMBoardSection parent, string axisType)
			: base(parent.Factory, parent, new ZQuery(BMBoardSectionChannelSchema.MSC_Axis, axisType), BMBoardSectionChannelSchema.MSC_MS_Section)
		{
			this.axisType = axisType;
		}

		readonly string axisType;

		public bool IsMissingChannelAdded { get; set; }

		protected override void SetDefaultsForNewElementCore(BMBoardSectionChannel newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.MSC_Sequence = this.MaxOrDefault(c => c.MSC_Sequence) + 1;
			newElement.MSC_Axis = axisType;
		}

		protected override void SetRelationshipDefaultsForElementCore(BMBoardSectionChannel newElement, bool throwIfRelationshipNotSupported)
		{
			newElement.IsAdded = IsMissingChannelAdded;
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { IsMissingChannelAdded };
		}
	}
}
