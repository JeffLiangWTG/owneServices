//  ___    _ _   ____                _   _   _     _     
// / _ \(_) | | |  _ \ ___  __ _  __| | | |_| |__ (_)___ 
//| | | | | | | | |_) / _ \/ _` |/ _` | | __| '_ \| / __|
//| |_| | |_|_| |  _ <  __/ (_| | (_| | | |_| | | | \__ \
// \___/|_(_|_) |_| \_\___|\__,_|\__,_|  \__|_| |_|_|___/
// Don't Make changes to this file manually, add transformations to ShelfCheckinMapper.txt
using System.Collections.Generic;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	static class Mapper
	{
		public static IEnumerable<Mapping> GetMappings()
		{
			//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
			yield return Mapping.New<Enterprise.DocumentVisualizer.DataTransformation.SeaBookingRequestV1ToV2Transformation>(new VersionLabel(2,0));
			yield break;
		}
	}
}