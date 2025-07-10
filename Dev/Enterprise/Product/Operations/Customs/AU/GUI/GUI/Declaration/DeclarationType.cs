using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	/// <summary>
	/// the DeclarationType in Customs.GUI uses a number as an ID.
	/// 
	/// This is used for grid layout context to persist column layout for each of these contexts 
	/// like 'export invoice line grid layout' vs 'import invoice line grid layout'. 
	/// Changing the names will result in losing the user settings that have been persisted. 
	/// </summary>
	[CodeAlive("Used in AUOtherSupplierHeaderUserControl, AUQuarantineSupplierHeaderUserControl")]
	public enum DeclarationType
	{
		EdificeImport,
		Quarantine
	}
}
