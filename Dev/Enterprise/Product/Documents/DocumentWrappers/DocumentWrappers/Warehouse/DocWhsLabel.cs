using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsLabel : DocumentWrapper
	{
		#region Constructors

		protected DocWhsLabel(WhsLabel label, BusinessObjectFactory factoryToWrap)
			: base(label, factoryToWrap)
		{
		}

		#endregion

		#region New

		public static DocWhsLabel New(WhsLabel label, BusinessObjectFactory factoryToWrap)
		{
			return (label != null ? new DocWhsLabel(label, factoryToWrap) : null);
		}

		#endregion

		#region Properties

		public ZInt LabelNumber
		{
			get { return Label.Number; }
		}

		public ZString LabelStringBarcode
		{
			get
			{
				TextBarcode barcode = new TextBarcode(this.LabelString);
				return barcode.TextAs128sFontString;
			}
		}

		public ZString LabelString
		{
			get { return Label.String; }
		}

		#endregion

		#region Implementation

		public override string ToString()
		{
			return LabelNumber.ToString();
		}

		protected WhsLabel Label
		{
			get { return (WhsLabel)WrappedObject; }
		}

		#endregion
	}

	public class WhsLabel : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public WhsLabel()
		{
		}

		public WhsLabel(ZString palletID)
		{
			String = palletID;
		}

		#endregion

		public static new ZString TableName
		{
			get { return "WhsLabel"; }
		}

		public ZInt Number;
		public ZString String;
	}
}
