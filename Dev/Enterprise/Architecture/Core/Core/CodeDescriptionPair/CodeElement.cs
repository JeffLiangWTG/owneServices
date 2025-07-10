using CargoWise.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public class CodeElement : CodeDescriptionPair, ICodeDescription
	{
		public CodeElement(object pk, string code, string description) : base(code, description)
		{
			fPK = pk;
		}

		public CodeElement(object pk, MultilingualString code, MultilingualString description)
			: base(code, description)
		{
			fPK = pk;
		}

		#region PK

		protected readonly object fPK;

		public object PK
		{
			get { return fPK; }
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		#endregion
	}
}
