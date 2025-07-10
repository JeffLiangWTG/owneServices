using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocDeclarationDocument : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocDeclarationDocument(ZString name, ZBool isRequired, ZBool isReceived)
		{
			fName = name;
			fIsRequired = isRequired;
			fIsReceived = isReceived;
		}

		public static new ZString TableName
		{
			get { return "DocDeclarationDocument"; }
		}

		public ZString Name
		{
			get { return fName; }
		}

		public ZBool IsRequired
		{
			get { return fIsRequired; }
		}

		public ZBool IsReceived
		{
			get { return fIsReceived; }
		}

		#region Implementation

		protected ZString fName;
		protected ZBool fIsRequired;
		protected ZBool fIsReceived;

		#endregion
	}
}
