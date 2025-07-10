function clearUserNameAndCompanyName() {
	localStorage.removeItem("loginName");
	localStorage.removeItem("companyCode");
}

function saveUserName(userName) {
	if (typeof userName === 'string' && userName !== '') {
		localStorage.setItem("loginName", userName);
	}
	else {
		localStorage.removeItem("loginName");
	}
}

function saveCompanyCode(companyCode) {
	if (typeof companyCode === 'string' && companyCode !== '') {
		localStorage.setItem("companyCode", companyCode);
	}
	else {
		localStorage.removeItem("companyCode");
	}
}

function getUserName() {
	const loginName = localStorage.getItem("loginName");
	if (loginName) {
		return loginName;
	}
	return '';
}

function getCompanyCode() {
	const companyCode = localStorage.getItem("companyCode");
	if (companyCode) {
		return companyCode;
	}
	return '';
}
