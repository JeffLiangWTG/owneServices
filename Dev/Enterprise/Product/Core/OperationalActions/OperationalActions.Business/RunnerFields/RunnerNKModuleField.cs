using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerNKModuleField : RunnerTextField<OperationalActionNKModuleFieldSupporter>
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : RunnerField<OperationalActionCodeFieldSupporter, ZString>.Schema
		{
			public const string Property_List = "Property_List";
		}

		#endregion

		public RunnerNKModuleField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionNKModuleFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			ListValidation.ErrorIfInvalidCode(PropertyInfo, Property_List);
		}

		public IBusinessObjectCollection Property_List
		{
			get { return property_List ?? (property_List = FieldSupporter.ConstructCollection(Factory)); }
		}
		IBusinessObjectCollection property_List;
	}
}
