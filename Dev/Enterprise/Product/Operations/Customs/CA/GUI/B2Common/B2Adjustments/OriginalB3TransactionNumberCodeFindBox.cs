using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI
{
	public class OriginalB3TransactionNumberCodeFindBox : ZCodeFindBox, IFindBox
	{
		string IFindBox.Code
		{
			get { return base.Code; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					var originalB3 = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, value));
					if (originalB3 != null)
					{
						base.Code = originalB3.DeclarationNumber;
					}
				}
			}
		}

		protected override void ShowEditOrViewForm()
		{
			// disable showView or Edit Form
		}

		BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory fFactory;
	}
}
