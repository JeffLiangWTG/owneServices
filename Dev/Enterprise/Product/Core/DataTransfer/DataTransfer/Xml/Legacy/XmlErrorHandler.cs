using System.ComponentModel;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class XmlErrorHandler
	{
		public bool HasErrors
		{
			get { return Errors != null && Errors.Count > 0; }
		}

		internal ErrorsCollection Errors
		{
			get
			{
				if (fErrors == null)
				{
					fErrors = new ErrorsCollection();
				}

				return fErrors;
			}
		}

		protected void ShowCompileErrors(object sender, ValidationEventArgs args)
		{
			Errors.Add(args.Message);
		}

		internal ErrorsCollection fErrors;
	}
}
