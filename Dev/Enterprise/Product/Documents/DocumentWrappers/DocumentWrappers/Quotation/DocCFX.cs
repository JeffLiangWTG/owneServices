using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor, AllowNoStaticNew]
	public class DocCFX : DocumentWrapper, IObsoleteValidation
	{
		public DocCFX(ZString name, ZDecimal value)
		{
			this.Name = name;
			this.Value = value;
		}

		public override string ToString()
		{
			return GetType().ToString();
		}

		public ZString Name { get; set; }
		public ZDecimal Value { get; set; }
	}
}
