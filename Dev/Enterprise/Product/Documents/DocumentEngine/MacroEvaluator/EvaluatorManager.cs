using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.MacroEvaluator
{
	public abstract class EvaluatorManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Output = "Output";
			public const int OutputMaxLength = 32767;
			public const string Macro = "Macro";
			public const int MacroMaxLength = 32767;
		}

		#endregion

		public EvaluatorManager(BusinessObjectFactory factory, IDocumentSupportable parent, string macro = "")
			: base(factory)
		{
			Argument.NotNull(parent, "parent");
			Parent = parent;
			Macro = macro;
		}

		protected IDocumentSupportable Parent { get; set; }

		#region Macro

		[MaxLength(Schema.MacroMaxLength)]
		public ZString Macro
		{
			get { return macro; }
			set { SetNonPersistentPropertyValue<ZString>(MacroInfo, ref macro, value); }
		}
		ZString macro = ZString.Empty;

		public ZPropertyInfo MacroInfo
		{
			get { return GetZPropertyInfo(Schema.Macro); }
		}

		#endregion

		#region Output

		[MaxLength(Schema.OutputMaxLength)]
		public ZString Output
		{
			get { return output; }
			set { SetNonPersistentPropertyValue<ZString>(OutputInfo, ref output, value); }
		}
		ZString output = ZString.Empty;

		public ZPropertyInfo OutputInfo
		{
			get { return GetZPropertyInfo(Schema.Output); }
		}

		#endregion

		#region Evaluate

		public abstract ZString Evaluate();

		#endregion

		#region Abstract properties

		public abstract bool HasDataContext { get; }

		public abstract ResourceStringData Name { get; }

		public abstract ResourceStringData MacroDescription { get; }

		#endregion
	}
}
