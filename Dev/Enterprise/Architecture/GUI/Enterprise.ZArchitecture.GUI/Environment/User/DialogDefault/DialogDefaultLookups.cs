using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Core.DialogDefault
{
	public class StmDialogDefaultLookups : ZLookups
	{
		public StmDialogDefaultLookups(StmDialogDefault parent)
			: base(parent)
		{ }

		public new StmDialogDefault Parent { get { return (StmDialogDefault)base.Parent; } }

		public IActiveBusinessObjectCollection Owners
		{
			get
			{
				switch (Parent.SDD_Level)
				{
					case DialogDefaultLevel.Codes.Company:
						return (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbCompanyCollection>("IGlbCompanyCollection", Factory);
					case DialogDefaultLevel.Codes.User:
						return (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", Factory);
					default:
						return null; //Nothing to look up for globals
				}
			}
		}

		public ICollection DialogIdentifier { get { return null; } }
		public CodeDescriptionPairList Level { get { return DialogDefaultLevel.DialogDefaultLevelsForCurrentUser; } }
	}
}
