import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { lastValueFrom  } from 'rxjs';
import { Config } from "../model/config";

@Injectable()
export class ConfigService {
	config: Config = {
		mockEndpointsUrl: ''
	};

	constructor(private http: HttpClient) {}

	loadConfig() {
		return lastValueFrom(this.http
		.get<Config>('./assets/config/app-config.json'))
		.then(config => {
			this.config = config;
		});
	}
}