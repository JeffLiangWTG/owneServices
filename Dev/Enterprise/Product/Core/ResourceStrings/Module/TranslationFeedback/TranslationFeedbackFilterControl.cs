using System;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module
{
	public partial class TranslationFeedbackFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public TranslationFeedbackFilterControl()
		{
			InitializeComponent();
		}

		public TranslationFeedbackFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
			new RowResizer(grid, StmTranslationFeedback.Schema.XT_Source, StmTranslationFeedback.Schema.XT_SuggestedTranslation);
			new TranslationFeedbackDiffHighlighter(grid);
		}
	}
}
