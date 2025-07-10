using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Support
{
	[System.Diagnostics.DebuggerDisplay("Name = {name}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class ActionMethodProviderID : ICodeDescription
	{
		internal ActionMethodProviderID(ZGuid guid, MultilingualString name, string providerFullName)
		{
			this.guid = guid;
			this.name = name;
			this.providerFullName = providerFullName ?? string.Empty;
		}

		public ZGuid Guid
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return guid; }
		}

		public MultilingualString Name
		{
			get { return name; }
		}

		public string ProviderFullName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return providerFullName; }
		}

		#region ICodeDescription Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string ICodeDescription.Code
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return Name; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string ICodeDescription.Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return ""; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICodeDescription.PK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return guid; }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ZGuid guid;

		readonly MultilingualString name;

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string providerFullName;
	}
}
