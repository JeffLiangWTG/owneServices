using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Make this class a property of a component to prevent successful compilation of the component if
	/// there is an error on the configuration of the component. The compilation error will alert the user
	/// to some text that should instruct the user how to fix the error.
	/// </summary>
	[DefaultValue(null)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializer(typeof(CompileTimeComponentValidationCodeDomSerializer), typeof(CodeDomSerializer))]
	[DebuggerDisplay("Message={Message}")]
	public class CompileTimeComponentValidation
	{
		public CompileTimeComponentValidation(string message)
		{
			Message = message;
		}

		[Obsolete("There are errors on your component, double click this item to view", true)]
		public static CompileTimeComponentValidation Error(string message)
		{
			return new CompileTimeComponentValidation(message);
		}

		/// <summary>
		/// The message that will appear at the point of the compilation error.
		/// </summary>
		public string Message { get; private set; }
	}
}
