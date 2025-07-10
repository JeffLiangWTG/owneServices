using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public class IDDImputationSheetWrapper<T> : DocBaseWrapper, IIDDImputationSheet
	{
		public IDDImputationSheetWrapper(T supportingDocument, BusinessObjectFactory factory) : base(supportingDocument, factory)
		{
		}

		public virtual ZString GoodsItemNumber => ZString.Empty;

		public virtual ZString DocumentType => ZString.Empty;

		public virtual ZString DocumentReference => ZString.Empty;

		public virtual ZString LineItemNumber => ZString.Empty;

		public virtual ZString Information => ZString.Empty;

		public virtual ZString Quantity => ZString.Empty;

		public virtual ZString MeasurementUnitAndQualifier => ZString.Empty;

		public virtual ZString Amount => ZString.Empty;

		public virtual ZString Currency => ZString.Empty;
	}
}
