using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerCodeField : RunnerTextField<OperationalActionCodeFieldSupporter>
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : RunnerField<OperationalActionCodeFieldSupporter, ZString>.Schema
		{
			public const string Property_List = "Property_List";
		}

		#endregion

		public RunnerCodeField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionCodeFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			ListValidation.ErrorIfInvalidCode(PropertyInfo, Property_List);
		}

		public ReadOnlyCodeDescriptionPairList Property_List
		{
			get { return FieldSupporter.List; }
		}
	}
}
