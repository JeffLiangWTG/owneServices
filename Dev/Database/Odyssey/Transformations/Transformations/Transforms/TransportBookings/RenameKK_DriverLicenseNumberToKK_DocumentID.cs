using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransportBookings
{
	public sealed class RenameKK_DriverLicenseNumberToKK_DocumentID : RenameColumnTransformation
	{
		public const string OldColumnName = "KK_DriverLicenseNumber";
		public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
		{
			get
			{
				yield return new RenameColumnTransformationInfo(DtbBookingConfirmationSchema.Constants.TableName, OldColumnName, DtbBookingConfirmationSchema.Constants.KK_DocumentID);
			}
		}
	}
}
