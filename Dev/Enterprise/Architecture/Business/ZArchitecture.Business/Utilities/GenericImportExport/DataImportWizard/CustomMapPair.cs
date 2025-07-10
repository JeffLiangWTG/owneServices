using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class CustomMapPair : NonPersistentBusinessObject, IObsoleteValidation, ICodeDescription
	{
		public CustomMapPair()
		{
		}

		#region Properties

		#region Input

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString Input
		{
			get { return input; }
			set { SetNonPersistentPropertyValue(InputInfo, ref input, value); }
		}

		ZString input;

		public ZPropertyInfo InputInfo
		{
			get { return GetZPropertyInfo(nameof(Input)); }
		}

		#endregion

		#region Output

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString Output
		{
			get { return output; }
			set { SetNonPersistentPropertyValue(OutputInfo, ref output, value); }
		}

		ZString output;

		public ZPropertyInfo OutputInfo
		{
			get { return GetZPropertyInfo(nameof(Output)); }
		}

		#endregion

		#endregion

		#region ICodeDescription Members

		string ICodeDescription.Code
		{
			get { return Input; }
		}

		string ICodeDescription.Description
		{
			get { return Output; }
		}

		object ICodeDescription.PK
		{
			get { return null; }
		}

		#endregion
	}
}
