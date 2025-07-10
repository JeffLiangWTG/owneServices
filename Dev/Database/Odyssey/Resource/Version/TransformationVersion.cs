namespace Enterprise.DbUpgrader.Resource.Version
{
	public static class TransformationVersion
	{
		/// <summary>
		///  _____                _   _______ _     _       ______ _          _   _
		/// |  __ \              | | |__   __| |   (_)     |  ____(_)        | | | |
		/// | |__) |___  __ _  __| |    | |  | |__  _ ___  | |__   _ _ __ ___| |_| |
		/// |  _  // _ \/ _` |/ _` |    | |  | '_ \| / __| |  __| | | '__/ __| __| |
		/// | | \ \  __/ (_| | (_| |    | |  | | | | \__ \ | |    | | |  \__ \ |_|_|
		/// |_|  \_\___|\__,_|\__,_|    |_|  |_| |_|_|___/ |_|    |_|_|  |___/\__(_)
		///
		/// *******************************************************************
		/// MINOR VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
		/// MAJOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA
		/// *******************************************************************
		/// THIS FILE SHOULD ALWAYS BE CHECKED OUT EXCLUSIVELY (LOCK)
		/// *******************************************************************
		/// CHECK Enterprise.DbUpgrader.Transformation.DataModification.Mapper 
		/// FOR INSTRUCTIONS ON ADDING DATA TRANSFORMATIONS
		/// *******************************************************************
		/// </summary>
		public static readonly VersionLabel ApplicationNumber = new VersionLabel(8280, 0);
	}
}
