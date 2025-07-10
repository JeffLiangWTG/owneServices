using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSPackageValidation : CusInvPackValidation
	{
		public EMCSPackageValidation(EMCSPackage parent) : base(parent)
		{
		}

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B5_UnitTypeInfo);
		}

		protected override void CheckB5_UnitCount()
		{
			base.CheckB5_UnitCount();

			if (Parent.IsCountable())
			{
				if (Parent.B5_UnitCount.IsEmpty)
				{
					Parent.B5_UnitCountInfo.AddMessageError(ResString.GetMultilingualString("C86B3287-B5C1-462F-B0F7-B24DF57F1311", "No of Packages can't be 0."));
				}
				else
				{
					CompareValidation.CheckWithinRange(Parent.B5_UnitCountInfo, 1, 999999999999999);
				}
			}
			else
			{
				if (!Parent.B5_UnitCount.IsEmpty && !Parent.B5_UnitType.IsEmpty)
				{
					Parent.B5_UnitCountInfo.AddMessageError(ResString.GetMultilingualString("3C414E11-9C4F-4504-8C8A-2640C5773D83", "{0} is not a countable package unit for EMCS.", Parent.B5_UnitType));
				}
			}

			CheckIsMainPack(Parent);
			CheckPackageIsLinked(Parent);
		}

		void CheckIsMainPack(EMCSPackage parent)
		{
			var errorMessage = ResString.GetMultilingualString("F2F4B92C-D4C3-4CE6-ABB2-344064E9A575", "Please specify the Main Pack Line for this Package.");
			var declaration = parent.Parent;

			var mainPackLines = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Where(l => l.ZG_IsMainPack);

			if (!mainPackLines.Any())
			{
				parent.B5_UnitCountInfo.AddMessageError(errorMessage);
			}
			else
			{
				var mainPackExists = mainPackLines.Any(l => l.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Any(p => p.Package == parent && p.IsForInvoiceLine));
				if (!mainPackExists)
				{
					parent.B5_UnitCountInfo.AddMessageError(errorMessage);
				}
			}
		}

		void CheckPackageIsLinked(EMCSPackage parent)
		{
			var errorMessage = ResString.GetMultilingualString("65C03E67-34E3-4348-B748-D0F15DF1AFF9", "Package must be linked to at least one Line.");
			var declaration = parent.Parent;

			var linkedLine = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Any(l => l.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Any(p => p.Package == parent && p.IsForInvoiceLine));

			if (!linkedLine)
			{
				parent.B5_UnitCountInfo.AddMessageError(errorMessage);
			}
		}

		protected override void CheckB5_MarksAndNumbers()
		{
			base.CheckB5_MarksAndNumbers();

			if (IsMarksAndNumbersRequired && Parent.B5_MarksAndNumbers.IsEmpty)
			{
				var targetInfo = Parent.B5_MarksAndNumbersInfo;
				targetInfo.AddMessageError(Res.GetString("F82EC808-0F8C-468F-9F1E-6D2D6CC2FB2C", "You have not entered {0}.", targetInfo.HumanReadableName));
			}
		}

		protected virtual bool IsMarksAndNumbersRequired => true;

		protected new EMCSPackage Parent => (EMCSPackage)base.Parent;
	}
}
