using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	public class DocBaseWrapperForTesting : DocBaseWrapper
	{
		public DocBaseWrapperForTesting(BusinessObject bizObj, BusinessObjectFactory factoryToWrap)
			: base(bizObj, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		protected override Image GetCompanyLogoFallback()
		{
			return fBranchImage ?? base.GetCompanyLogoFallback();
		}

		public string GetWeightVolumeDisplayOptionTestMethod()
		{
			return base.GetWeightVolumeDisplayOption();
		}

		public ZString GetNotesTestMethod(ZString description, ZString context, Notes bizONotes, ZString direction)
		{
			return base.GetNotes(description, context, bizONotes, direction);
		}

		public ZString GetNotesTestMethod(ZString description, ZString context, Notes bizONotes)
		{
			return base.GetNotes(description, context, bizONotes);
		}

		public ZString GetNotesTestMethod(ZString description, BusinessObject bizO)
		{
			return base.GetNotes(description, bizO);
		}

		public ZString GetNotesTestMethod(ZString description, Notes notes)
		{
			return base.GetNotes(description, notes);
		}

		public ZString GetAllNotesTestMethod(BusinessObject bizo)
		{
			return base.GetAllNotes(bizo);
		}

		public ZString GetAllNotesTestMethod(Notes notes)
		{
			return base.GetAllNotes(notes);
		}

		public ZString[] GetAllNotesInStringArrayTestMethod(BusinessObject bizo)
		{
			return base.GetAllNotesInStringArray(bizo);
		}

		public ZString[] GetAllNotesInStringArrayTestMethod(Notes notes)
		{
			return base.GetAllNotesInStringArray(notes);
		}

		Image fBranchImage;
		public void SetBranchImage(Image imageToSet)
		{
			fBranchImage = imageToSet;
		}
	}
}
