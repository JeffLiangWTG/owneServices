using System;
using Enterprise.Core;

namespace Enterprise.ExcelTemplates
{
	public class DataContextAttribute : Attribute
	{
		/// <summary>
		/// Assigns a DataContext, which indicates what type of PK is required for the Document.
		/// </summary>
		public DataContextAttribute(Constants.DataContext context)
		{
			this.Context = context;
		}

		public readonly Constants.DataContext Context;
	}
}
