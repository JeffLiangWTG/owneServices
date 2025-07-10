using System.ComponentModel;
using System.Diagnostics;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	[ImmutableObject(true)]
	[DebuggerDisplay("{Code}-{Description}")]
	public class Event : CodeDescriptionPair, ICodeDescription
	{
		protected internal Event(string code, MultilingualString description, ZGuid pk)
			: base(code, description)
		{
			PK = pk;
		}

		public readonly ZGuid PK;

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		#endregion
	}
}
