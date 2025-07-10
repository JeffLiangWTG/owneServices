
namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	using System;
	using CargoWise.Common;

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class EventParameterAttribute : Attribute
	{
		/// <summary>
		///		Initializes a new instance of <see cref="EventParameterAttribute"/>.
		/// </summary>
		/// <param name="code">
		///		The code that will be used for storing the parameter. 
		/// </param>
		public EventParameterAttribute(string code)
		{
			Code = Argument.NotNullOrEmpty(code, "code");
		}

		public string Code { get; private set; }
	}
}
