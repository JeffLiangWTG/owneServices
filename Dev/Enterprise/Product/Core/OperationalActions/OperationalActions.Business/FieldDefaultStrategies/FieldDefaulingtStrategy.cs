using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("{Code} - {Description}")]
	abstract class FieldDefaultingStrategy<FieldSupporterT> : IFieldDefaultingStrategy
		where FieldSupporterT : OperationalActionFieldSupporter
	{
		protected FieldDefaultingStrategy(string code, string description, FieldSupporterT fieldSupporter)
		{
			if (code == null)
			{
				throw new ArgumentNullException(nameof(code));
			}

			if (description == null)
			{
				throw new ArgumentNullException(nameof(description));
			}

			if (fieldSupporter == null)
			{
				throw new ArgumentNullException(nameof(fieldSupporter));
			}

			if (code.Length == 0)
			{
				throw new ArgumentException("code cannot be empty", nameof(code));
			}

			this.code = code;
			this.description = description;
			this.fieldSupporter = fieldSupporter;
		}

		protected static string FixedTextDescription
		{
			get { return Res.GetString("98f83fea-3d09-430f-9f77-a5f874bb0ae9", "Fixed Text"); }
		}

		protected FieldSupporterT FieldSupporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fieldSupporter; }
		}

		#region IFieldDefaultStrategy Members

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public virtual IList GetBoundCollection(BusinessObjectFactory factory)
		{
			return new ReadOnlyCodeDescriptionPairList();
		}

		public abstract FieldType DetailFieldType { get; }
		public abstract int DetailMaxLength { get; }
		public abstract IZType GetDefaultValue(string detail);

		#endregion

		#region ICodeDescription Members

		public string Code
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return code; }
		}

		public string Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return description; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		object ICodeDescription.PK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return null; }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string code;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string description;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly FieldSupporterT fieldSupporter;
	}
}
